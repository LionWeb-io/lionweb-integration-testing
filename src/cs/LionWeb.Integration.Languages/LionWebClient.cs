using System.Collections.Concurrent;
using System.Diagnostics;
using LionWeb.Core;
using LionWeb.Core.M1;
using LionWeb.Core.M1.Event.Partition;
using LionWeb.Core.M3;
using LionWeb.Core.Serialization;
using ParticipationId = string;
using CommandId = string;

namespace LionWeb.Integration.Languages;

public class LionWebClient
{
    private readonly string _name;
    private readonly Func<string, Task> _send;
    private readonly DeltaProtocolPartitionEventReceiver _eventReceiver;
    private readonly DeltaSerializer _deltaSerializer;
    private readonly PartitionEventToDeltaCommandMapper _mapper;
    private readonly ConcurrentDictionary<CommandId, int> _ownCommands = [];
    
    private ParticipationId? _participationId;
    private long _messageCount;

    public long MessageCount => Interlocked.Read(ref _messageCount);

    public LionWebClient(LionWebVersions lionWebVersion, List<Language> languages, string name,
        IPartitionInstance partition, Func<string, Task> send)
    {
        _name = name;
        _send = send;
        _mapper = new PartitionEventToDeltaCommandMapper(new CommandIdProvider(), lionWebVersion);
        
        Dictionary<string, IReadableNode> sharedNodeMap = [];
        var partitionEventHandler = new PartitionEventHandler(name);
        DeserializerBuilder deserializerBuilder = new DeserializerBuilder()
                .WithLionWebVersion(lionWebVersion)
                .WithLanguages(languages)
                .WithHandler(new ReceiverDeserializerHandler())
            ;
        Dictionary<CompressedMetaPointer, IKeyed>
            sharedKeyedMap = DeltaCommandToDeltaEventMapper.BuildSharedKeyMap(languages);
        _eventReceiver = new DeltaProtocolPartitionEventReceiver(
            partitionEventHandler,
            sharedNodeMap,
            sharedKeyedMap,
            deserializerBuilder
        );
        var replicator = new PartitionEventReplicator(partition, sharedNodeMap);
        replicator.ReplicateFrom(partitionEventHandler);
        _deltaSerializer = new DeltaSerializer();

        IPartitionPublisher publisher = replicator;
        publisher.Subscribe<IPartitionEvent>(SendPartitionEventToRepository);
    }

    private void SendPartitionEventToRepository(object? sender, IPartitionEvent? partitionEvent)
    {
        if (partitionEvent == null)
            return;

        IDeltaCommand deltaCommand = _mapper.Map(partitionEvent);

        Send(deltaCommand);
    }
    
    public async Task Send(IDeltaContent deltaContent)
    {
        if (deltaContent.RequiresParticipationId)
            deltaContent.InternalParticipationId = _participationId;
        
        if (deltaContent is IDeltaCommand { CommandId: { } commandId })
            _ownCommands.TryAdd(commandId, 1);
        
        Debug.WriteLine($"{_name}: sending: {deltaContent.GetType()}");
        await _send.Invoke(_deltaSerializer.Serialize(deltaContent));
    }
    
    public void Receive(string msg)
    {
        try
        {
            IDeltaContent content = _deltaSerializer.Deserialize<IDeltaContent>(msg);
            Interlocked.Increment(ref _messageCount);
            switch (content)
            {
                case IDeltaEvent deltaEvent:
                    CommandSource? commandSource = null;
                    if (deltaEvent is ISingleDeltaEvent singleDeltaEvent)
                    {
                        commandSource = singleDeltaEvent.OriginCommands.FirstOrDefault();
                        if (singleDeltaEvent.OriginCommands.All(cmd =>
                                _participationId == cmd.ParticipationId &&
                                _ownCommands.TryRemove(cmd.CommandId, out _)))
                        {
                            Debug.WriteLine(
                                $"{_name}: ignoring own event: {deltaEvent.GetType()}({commandSource},{deltaEvent.EventSequenceNumber})");
                            return;
                        }
                    }

                    Debug.WriteLine(
                        $"{_name}: received event: {deltaEvent.GetType()}({commandSource},{deltaEvent.EventSequenceNumber})");
                    deltaEvent.InternalParticipationId = commandSource?.ParticipationId;
                    _eventReceiver.Receive(deltaEvent);
                    break;

                case SignOnResponse signOnResponse:
                    Debug.WriteLine($"{_name}: received {nameof(SignOnResponse)}: {signOnResponse})");
                    _participationId = signOnResponse.ParticipationId;
                    break;

                default:
                    Debug.WriteLine($"{_name}: ignoring received: {content.GetType()}({content.InternalParticipationId})");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}

public class CommandIdProvider : ICommandIdProvider
{
    private int nextId = 0;
    public string Create() => (++nextId).ToString();
}
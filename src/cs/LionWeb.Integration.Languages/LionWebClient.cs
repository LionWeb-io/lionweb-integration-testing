using LionWeb.Core;
using LionWeb.Core.M1;
using LionWeb.Core.M1.Event.Partition;
using LionWeb.Core.M3;
using LionWeb.Core.Serialization;
using ParticipationId = string;

namespace LionWeb.Integration.Languages;

public class LionWebClient
{
    private readonly string _name;
    private readonly Action<string> _send;
    private readonly DeltaProtocolPartitionEventReceiver _eventReceiver;
    private readonly DeltaSerializer _deltaSerializer;
    private readonly PartitionEventToDeltaCommandMapper _mapper;
    private ParticipationId? _participationId;

    private long _messageCount;

    public long MessageCount => Interlocked.Read(ref _messageCount);

    public LionWebClient(LionWebVersions lionWebVersion, List<Language> languages, string name,
        IPartitionInstance partition, Action<string> send)
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
    
    public void Send(IDeltaContent deltaContent)
    {
        if (deltaContent.RequiresParticipationId)
            deltaContent.InternalParticipationId = _participationId;
        Console.WriteLine($"{_name}: sending: {deltaContent.GetType()}");
        _send(_deltaSerializer.Serialize(deltaContent));
    }
    
    public void Receive(string msg)
    {
        try
        {
            // Console.WriteLine($"{_name} received event: {msg}");
            IDeltaContent content = _deltaSerializer.Deserialize<IDeltaContent>(msg);
            Interlocked.Increment(ref _messageCount);
            switch (content)
            {
                case IDeltaEvent @event:
                    var commandSource = @event is ISingleDeltaEvent { OriginCommands: { } cmds } ? cmds.First() : null;

                    Console.WriteLine(
                        $"{_name}: received event: {@event.GetType()}({commandSource},{@event.EventSequenceNumber})");
                    @event.InternalParticipationId = commandSource?.ParticipationId;
                    _eventReceiver.Receive(@event);
                    break;

                case SignOnResponse signOnResponse:
                    Console.WriteLine($"{_name}: received {nameof(SignOnResponse)}: {signOnResponse})");
                    _participationId = signOnResponse.ParticipationId;
                    break;

                default:
                    Console.WriteLine($"{_name}: ignoring received: {content.GetType()}({content.InternalParticipationId})");
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

public class CommandIdProvider : ICommandIdProvider
{
    private int nextId = 0;
    public string Create() => (++nextId).ToString();
}
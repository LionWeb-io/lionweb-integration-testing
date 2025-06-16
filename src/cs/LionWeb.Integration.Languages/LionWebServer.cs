using System.Text.Json;
using LionWeb.Core;
using LionWeb.Core.M1;
using LionWeb.Core.M1.Event.Partition;
using LionWeb.Core.M3;
using LionWeb.Core.Serialization;
using ParticipationId = string;

namespace LionWeb.Integration.Languages;

public class LionWebServer
{
    private readonly string _name;
    private readonly Func<string, Task> _sendAll;
    private readonly Func<IClientInfo, string, Task> _send;
    private readonly DeltaProtocolPartitionCommandReceiver _commandReceiver;
    private readonly DeltaSerializer _deltaSerializer;
    private readonly PartitionEventToDeltaEventMapper _mapper;

    private long _messageCount;
    public long MessageCount => Interlocked.Read(ref _messageCount);

    public LionWebServer(LionWebVersions lionWebVersion, List<Language> languages, string name,
        IPartitionInstance partition, Func<string, Task> sendAll, Func<IClientInfo, string, Task> send)
    {
        _name = name;
        _sendAll = sendAll;
        _send = send;
        _mapper = new PartitionEventToDeltaEventMapper(new ExceptionParticipationIdProvider(), new EventSequenceNumberProvider(), lionWebVersion);
        
        Dictionary<string, IReadableNode> sharedNodeMap = [];
        var partitionEventHandler = new PartitionEventHandler(name);
        DeserializerBuilder deserializerBuilder = new DeserializerBuilder()
                .WithLionWebVersion(lionWebVersion)
                .WithLanguages(languages)
                .WithHandler(new ReceiverDeserializerHandler())
            ;
        Dictionary<CompressedMetaPointer, IKeyed>
            sharedKeyedMap = DeltaCommandToDeltaEventMapper.BuildSharedKeyMap(languages);
        _commandReceiver = new DeltaProtocolPartitionCommandReceiver(
            partitionEventHandler,
            sharedNodeMap,
            sharedKeyedMap,
            deserializerBuilder
        );
        var replicator = new RewritePartitionEventReplicator(partition, sharedNodeMap);
        replicator.ReplicateFrom(partitionEventHandler);
        _deltaSerializer = new DeltaSerializer();

        var publisher = replicator;
        publisher.Subscribe<IPartitionEvent>(SendPartitionEventToAllClients);
    }

    private void SendPartitionEventToAllClients(object? sender, IPartitionEvent? partitionEvent)
    {
        if (partitionEvent == null)
            return;

        IDeltaEvent deltaEvent = _mapper.Map(partitionEvent);
        SendAll(deltaEvent);
    }

    private async Task SendAll(IDeltaContent deltaContent)
    {
        switch (deltaContent)
        {
            case ISingleDeltaEvent deltaEvent:
                var commandSource = deltaEvent is ISingleDeltaEvent { OriginCommands: { } cmds }
                    ? cmds.First()
                    : null;
                Console.WriteLine(
                    $"{_name}: sending event: {deltaEvent.GetType()}({commandSource},{deltaEvent.EventSequenceNumber})");
                break;

            default:
                Console.WriteLine($"{_name}: sending: {deltaContent.GetType()}");
                break;
        }

        await _sendAll(_deltaSerializer.Serialize(deltaContent));
    }

    private async Task Send(IClientInfo clientInfo, IDeltaContent deltaContent)
    {
        Console.WriteLine($"{_name}: sending to {clientInfo}: {deltaContent.GetType()}");
        await _send(clientInfo, _deltaSerializer.Serialize(deltaContent));
    }

    public async Task Receive(IWebSocketMessage msg)
    {
        try
        {
            // Console.WriteLine($"{_name} received command: {msg}");
            var content = _deltaSerializer.Deserialize<IDeltaContent>(msg.MessageContent);
            content.InternalParticipationId = msg.ClientInfo.ParticipationId;
            Console.WriteLine(
                $"{_name}: received {content.GetType().Name} for {msg.ClientInfo.ParticipationId}: {content})");
            Interlocked.Increment(ref _messageCount);

            switch (content)
            {
                case IDeltaCommand command:
                    Console.WriteLine($"{_name}: received command: {command.GetType()}({command.CommandId})");
                    _commandReceiver.Receive(command);
                    break;

                case SignOnRequest signOnRequest:
                    Console.WriteLine(
                        $"{_name}: received {nameof(SignOnRequest)} for {msg.ClientInfo}: {signOnRequest})");
                    await Send(msg.ClientInfo,
                        new SignOnResponse(msg.ClientInfo.ParticipationId, signOnRequest.QueryId, null));
                    break;

                default:
                    Console.WriteLine($"{_name}: ignoring received: {content.GetType()}({content.InternalParticipationId})");
                    break;
            }
        }
        catch (JsonException e)
        {
            Console.WriteLine(msg.MessageContent);
            Console.WriteLine(e);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

public class ExceptionParticipationIdProvider : IParticipationIdProvider
{
    public string ParticipationId => throw new NotImplementedException();
}

internal class ReceiverDeserializerHandler : DeserializerExceptionHandler
{
    public override bool SkipDeserializingDependentNode(CompressedId id)
    {
        return false;
    }
}

public class EventSequenceNumberProvider : IEventSequenceNumberProvider
{
    private long next = 0;
    public long Create() => ++next;
}

public interface IWebSocketMessage
{
    IClientInfo ClientInfo { get; }
    string MessageContent { get; }
}

public interface IClientInfo
{
    ParticipationId ParticipationId { get; }
}
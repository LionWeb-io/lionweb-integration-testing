using LionWeb.Core;
using LionWeb.Core.M1;
using LionWeb.Core.M1.Event.Partition;
using LionWeb.Core.M3;
using LionWeb.Core.Serialization;

namespace LionWeb.Integration.Languages;

public class ClientReceiver
{
    private readonly LionWebVersions _lionWebVersion;
    private readonly string _name;
    private readonly Dictionary<String, IReadableNode> _sharedNodeMap;
    private readonly DeltaProtocolPartitionEventReceiver _eventReceiver;
    private readonly DeltaSerializer _deltaSerializer;
    private readonly IPartitionPublisher _publisher;

    private long _messageCount;
    private readonly CommandToEventMapper _commandToEventMapper;
    private readonly DeltaProtocolPartitionCommandSender _commandSender;

    public long MessageCount => Interlocked.Read(ref _messageCount);

    public ClientReceiver(LionWebVersions lionWebVersion, List<Language> languages, string name,
        IPartitionInstance partition,
        bool replicateChanges = false)
    {
        _lionWebVersion = lionWebVersion;
        _name = name;
        _sharedNodeMap = [];
        var partitionEventHandler = new PartitionEventHandler(name);
        DeserializerBuilder deserializerBuilder = new DeserializerBuilder()
                .WithLionWebVersion(lionWebVersion)
                .WithLanguages(languages)
                .WithHandler(new ReceiverDeserializerHandler())
            ;
        Dictionary<CompressedMetaPointer, IKeyed>
            sharedKeyedMap = CommandToEventMapper.BuildSharedKeyMap(languages);
        _eventReceiver = new DeltaProtocolPartitionEventReceiver(
            partitionEventHandler,
            _sharedNodeMap,
            sharedKeyedMap,
            deserializerBuilder
        );
        var replicator = new PartitionEventReplicator(partition, _sharedNodeMap);
        replicator.ReplicateFrom(partitionEventHandler);
        _deltaSerializer = new DeltaSerializer();

        _publisher = replicateChanges ? partition.GetPublisher() : replicator;
        _commandToEventMapper = new CommandToEventMapper(_name, _sharedNodeMap);
        _commandSender = new DeltaProtocolPartitionCommandSender(_publisher, new CommandIdProvider(), _lionWebVersion);
    }

    private class ReceiverDeserializerHandler : DeserializerExceptionHandler
    {
        public override bool SkipDeserializingDependentNode(CompressedId id)
        {
            return false;
        }
    }

    public void Send(Action<string> action)
    {
        _commandSender.DeltaCommand += (sender, command) =>
        {
            Console.WriteLine($"{_name}: sending command: {command.GetType().Name}({command.CommandId})");
            var deltaSerializer = new DeltaSerializer();
            action(deltaSerializer.Serialize(command));
        };
    }

    public void Receive(string msg)
    {
        try
        {
            // Console.WriteLine($"{_name} received event: {msg}");
            var @event = _deltaSerializer.Deserialize<IDeltaEvent>(msg);
            Console.WriteLine($"{_name}: received event: {@event.GetType()}({@event.EventSequenceNumber})");
            _eventReceiver.Receive(@event);
            Interlocked.Increment(ref _messageCount);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
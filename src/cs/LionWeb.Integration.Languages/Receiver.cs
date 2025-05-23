using LionWeb.Core;
using LionWeb.Core.M1;
using LionWeb.Core.M1.Event.Partition;
using LionWeb.Core.M3;
using LionWeb.Core.Serialization;

namespace LionWeb.Integration.Languages;

public class Receiver
{
    private readonly LionWebVersions _lionWebVersion;
    private readonly string _name;
    private readonly Dictionary<String, IReadableNode> _sharedNodeMap;
    private readonly DeltaProtocolPartitionEventReceiver _eventReceiver;
    private readonly DeltaSerializer _deltaSerializer;
    private readonly IPartitionPublisher _publisher;

    private long _messageCount;

    public long MessageCount => Interlocked.Read(ref _messageCount);

    public Receiver(LionWebVersions lionWebVersion, List<Language> languages, string name, IPartitionInstance partition,
        bool replicateChanges = false)
    {
        _lionWebVersion = lionWebVersion;
        _name = name;
        _sharedNodeMap = [];
        var partitionEventHandler = new PartitionEventHandler(null);
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
        var commandToEventMapper = new CommandToEventMapper(_sharedNodeMap);

        var commandSender =
            new DeltaProtocolPartitionCommandSender(_publisher, new CommandIdProvider(), _lionWebVersion);

        commandSender.DeltaCommand += (sender, command) =>
        {
            var @event = commandToEventMapper.Map(command);

            Console.WriteLine($"{_name} sending event: {@event}");
            var deltaSerializer = new DeltaSerializer();
            action(deltaSerializer.Serialize(@event));
        };
    }

    public void Receive(string msg)
    {
        try
        {
            Console.WriteLine($"{_name} received: {msg}");
            var @event = _deltaSerializer.Deserialize<IDeltaEvent>(msg);
            _eventReceiver.Receive(@event);
            Interlocked.Increment(ref _messageCount);
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
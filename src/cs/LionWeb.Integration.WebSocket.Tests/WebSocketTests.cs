using System.Runtime.CompilerServices;
using System.Text.Json;
using LionWeb.Core;
using LionWeb.Core.M1.Event;
using LionWeb.Core.M3;
using LionWeb.Core.Serialization;
using LionWeb.Core.Utilities;
using LionWeb.Integration.Languages;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LionWeb.Integration.WebSocket.Tests;

[TestClass]
public class WebSocketTests : WebSocketClientTestBase
{
    [TestMethod]
    public void bla()
    {
        var childAdded = new ChildAdded(
            "parent", new MetaPointer("lang", "ver", "key"), 3,
            new DeltaSerializationChunk([
                new SerializedNode()
                {
                    Id = "documentation",
                    Classifier = new MetaPointer("key-Shapes", "1", "key-Documentation"),
                    Properties = [
                        new SerializedProperty()
                    {
                        Property = new MetaPointer("key-Shapes", "1", "key-technical"),
                        Value = null
                    },
                        new SerializedProperty() {
                        Property = new MetaPointer("key-Shapes", "1", "key-text"),
                        Value = null
                    },
                    ],
                    Containments = [],
                    References = [],
                    Annotations = [],
                    Parent = "a"
                }
            ]), 23, [new CommandSource("participationId", "commandId")], null);

        Console.WriteLine(childAdded);
        
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            TypeInfoResolver = new DeltaProtocolTypeResolver()
        };
        var serialized = JsonSerializer.Serialize<IDeltaContent>(childAdded, jsonSerializerOptions);

        Console.WriteLine(serialized);

        var deltaEvent = JsonSerializer.Deserialize<IDeltaEvent>(serialized, jsonSerializerOptions);
        Console.WriteLine(deltaEvent);
        Console.WriteLine((deltaEvent as ChildAdded)?.NewChild.Nodes.First());
    }

    [TestMethod, Timeout(3000)]
    public async Task Communication()
    {
        // var server = Server;
        // server.Received += (sender, msg) => Console.WriteLine($"server received: {msg}");

        var clientA = new WebSocketClient("A");
        clientA.Received += (sender, msg) => Console.WriteLine($"client A received: {msg}");

        var clientB = new WebSocketClient("B");
        clientB.Received += (sender, msg) => Console.WriteLine($"client B received: {msg}");

        var ipAddress = "localhost";
        var port = 42424;
        await clientA.ConnectToServer($"ws://{ipAddress}:{port}");
        await clientB.ConnectToServer($"ws://{ipAddress}:{port}");
        // await server.Send("hello from server");
        await clientA.Send("hello from client A");
        // await server.Send("bye from server");
        Thread.Sleep(100);
    }

    static IVersion2023_1 lionWebVersion = LionWebVersions.v2023_1;
    static List<Language> languages = [ShapesLanguage.Instance, lionWebVersion.BuiltIns, lionWebVersion.LionCore];

    [TestMethod, Timeout(13000)]
    public async Task Model()
    {
        var serverNode = new Geometry("a");
        // var server = Server;
        // Console.WriteLine(server);
        // {
        //     var receiverServer = new Receiver(lionWebVersion, languages, "server", serverNode, true);
        //     receiverServer.Send(s => server.Send(s));
        //     server.Received += (sender, msg) => receiverServer.Receive(msg);
        // }


        var clientAClone = (Geometry)new SameIdCloner([serverNode]).Clone()[serverNode];
        var clientA = new WebSocketClient("A");
        var receiverA = new ClientReceiver(lionWebVersion, languages, "client A", clientAClone);
        {
            receiverA.Send(s => clientA.Send(s));
            clientA.Received += (sender, msg) => receiverA.Receive(msg);
        }

        var clientBClone = (Geometry)new SameIdCloner([serverNode]).Clone()[serverNode];
        var clientB = new WebSocketClient("B");
        var receiverB = new ClientReceiver(lionWebVersion, languages, "client B", clientBClone);
        {
            receiverB.Send(s => clientB.Send(s));
            clientB.Received += (sender, msg) => receiverB.Receive(msg);
        }

        Console.WriteLine($"{nameof(clientAClone)}: Partition {clientAClone.PrintIdentity()}");
        Console.WriteLine($"{nameof(clientBClone)}: Partition {clientBClone.PrintIdentity()}");

        var ipAddress = "localhost";
        var port = 42424;
        await clientA.ConnectToServer($"ws://{ipAddress}:{port}");
        await clientB.ConnectToServer($"ws://{ipAddress}:{port}");

        clientBClone.Documentation = new Documentation("documentation");
        Console.WriteLine($"clientB Documentation {clientBClone.Documentation.PrintIdentity()}");

        while (receiverA.MessageCount < 1)
        {
            Thread.Sleep(100);
        }

        Console.WriteLine($"clientA Documentation {clientAClone.Documentation.PrintIdentity()}");
        clientAClone.Documentation.Text = "hello there";

        while (receiverA.MessageCount < 1 || receiverB.MessageCount < 2)
        {
            Thread.Sleep(100);
        }


        Console.WriteLine($"clientA Documentation {clientAClone.Documentation.PrintIdentity()}");
        Console.WriteLine($"clientB Documentation {clientBClone.Documentation.PrintIdentity()}");
        
        AssertEquals([clientAClone], [clientBClone]);
    }

    private void AssertEquals(IEnumerable<INode?> expected, IEnumerable<INode?> actual)
    {
        List<IDifference> differences = new Comparer(expected.ToList(), actual.ToList()).Compare().ToList();
        Assert.IsFalse(differences.Count != 0, differences.DescribeAll(new()));
    }
}
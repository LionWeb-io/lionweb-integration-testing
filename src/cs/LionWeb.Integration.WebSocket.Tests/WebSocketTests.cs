using System.Diagnostics;
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
                    Properties =
                    [
                        new SerializedProperty()
                        {
                            Property = new MetaPointer("key-Shapes", "1", "key-technical"),
                            Value = null
                        },
                        new SerializedProperty()
                        {
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
        clientA.Receive += (sender, msg) => Console.WriteLine($"client A received: {msg}");

        var clientB = new WebSocketClient("B");
        clientB.Receive += (sender, msg) => Console.WriteLine($"client B received: {msg}");

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
    private static readonly string _ipAddress = "localhost";
    private static readonly int _port = 42424;

    [TestMethod, Timeout(6000)]
    public async Task Model()
    {
        var serverNode = new Geometry("a");

        var aPartition = SameIdCloner.Clone(serverNode);
        var aClient = await ConnectWebSocket(aPartition, "A");

        var bPartition = SameIdCloner.Clone(serverNode);
        var bClient = await ConnectWebSocket(bPartition, "B");

        Debug.WriteLine($"{nameof(aPartition)}: Partition {aPartition.PrintIdentity()}");
        Debug.WriteLine($"{nameof(bPartition)}: Partition {bPartition.PrintIdentity()}");

        bPartition.Documentation = new Documentation("documentation");
        Debug.WriteLine($"clientB Documentation {bPartition.Documentation.PrintIdentity()}");

        aClient.WaitForCount(2);

        Debug.WriteLine($"clientA Documentation {aPartition.Documentation.PrintIdentity()}");
        aPartition.Documentation.Text = "hello there";

        bClient.WaitForCount(3);

        Debug.WriteLine($"clientA Documentation {aPartition.Documentation.PrintIdentity()}");
        Debug.WriteLine($"clientB Documentation {bPartition.Documentation.PrintIdentity()}");

        AssertEquals([aPartition], [bPartition]);
    }

    private static async Task<LionWebClient> ConnectWebSocket(Geometry clientAClone, string name)
    {
        var webSocket = new WebSocketClient(name);
        var lionWeb = new LionWebClient(lionWebVersion, languages, $"client_{name}", clientAClone, webSocket);
        await webSocket.ConnectToServer($"ws://{_ipAddress}:{_port}");
        await lionWeb.Send(new SignOnRequest("2025.1", IdUtils.NewId(), null));
        lionWeb.WaitForCount(1);
        return lionWeb;
    }

    private void AssertEquals(IEnumerable<INode?> expected, IEnumerable<INode?> actual)
    {
        List<IDifference> differences = new Comparer(expected.ToList(), actual.ToList()).Compare().ToList();
        Assert.IsTrue(differences.Count == 0, differences.DescribeAll(new()));
    }
}

static class ClientExtensions
{
    private const int SleepInterval = 100;

    public static void WaitForCount(this LionWebClient client, int count)
    {
        while (client.MessageCount < count)
        {
            Thread.Sleep(SleepInterval);
        }
    }
}
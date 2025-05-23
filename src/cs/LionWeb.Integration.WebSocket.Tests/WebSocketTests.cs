using LionWeb.Core;
using LionWeb.Core.M1.Event;
using LionWeb.Core.M3;
using LionWeb.Core.Utilities;
using LionWeb.Integration.Languages;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LionWeb.Integration.WebSocket.Tests;

[TestClass]
public class WebSocketTests : WebSocketClientTestBase
{
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
        var receiverA = new Receiver(lionWebVersion, languages, "client A", clientAClone);
        {
            receiverA.Send(s => clientA.Send(s));
            clientA.Received += (sender, msg) => receiverA.Receive(msg);
        }

        var clientBClone = (Geometry)new SameIdCloner([serverNode]).Clone()[serverNode];
        var clientB = new WebSocketClient("B");
        var receiverB = new Receiver(lionWebVersion, languages, "client B", clientBClone);
        {
            receiverB.Send(s => clientB.Send(s));
            clientB.Received += (sender, msg) => receiverB.Receive(msg);
        }

        var ipAddress = "localhost";
        var port = 42424;
        await clientA.ConnectToServer($"ws://{ipAddress}:{port}");
        await clientB.ConnectToServer($"ws://{ipAddress}:{port}");

        clientBClone.Documentation = new Documentation("documentation");

        // while (receiverA.MessageCount < 1)
        Thread.Sleep(1000);

        clientAClone.Documentation.Text = "hello there";

        // while (receiverA.MessageCount < 1 || receiverB.MessageCount < 2)
        Thread.Sleep(1000);

        AssertEquals([serverNode], [clientAClone]);
        AssertEquals([serverNode], [clientBClone]);
    }

    private void AssertEquals(IEnumerable<INode?> expected, IEnumerable<INode?> actual)
    {
        List<IDifference> differences = new Comparer(expected.ToList(), actual.ToList()).Compare().ToList();
        Assert.IsFalse(differences.Count != 0, differences.DescribeAll(new()));
    }
}
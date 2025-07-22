using System.Diagnostics;
using LionWeb.Core.M1.Event;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Server;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Tests.Server;

[TestFixture(ClientProcesses.CSharp)]
public class ReferenceServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds a reference to a specified child node
    /// </summary>
    [Test]
    public void AddReference()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_1", "AddReference_0_1_to_Containment_0_1");

        lionWebServer.WaitForReceived(3);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = new LinkTestConcept("containment_0_1"),
        };
        expected.Reference_0_1 = expected.Containment_0_1;

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Deletes a reference to a specified child node.
    /// </summary>
    [Test]
    public void DeleteReference()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_1", "AddReference_0_1_to_Containment_0_1", "DeleteReference_0_1");

        lionWebServer.WaitForReceived(4);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = new LinkTestConcept("containment_0_1"),
        };

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Changes the reference to a another child node
    /// </summary>
    [Test]
    public void ChangeReference()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_1", "AddContainment_1", 
            "AddReference_0_1_to_Containment_0_1", "AddReference_0_1_to_Containment_1");

        lionWebServer.WaitForReceived(5);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = new LinkTestConcept("containment_0_1"),
            Containment_1 = new LinkTestConcept("containment_1")
        };
        expected.Reference_0_1 = expected.Containment_1;

        AssertEquals(expected, serverPartition);
    }
}
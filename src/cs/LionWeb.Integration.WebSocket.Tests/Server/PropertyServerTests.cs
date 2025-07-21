using System.Diagnostics;
using LionWeb.Core.M1.Event;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Server;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Tests.Server;

[TestFixture(ClientProcesses.CSharp)]
public class PropertyServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds a new property to a server partition and verifies the change.
    /// </summary>
    [Test]
    public void AddProperty()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new DataTypeTestConcept("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", serverPartition.GetType().ToString(),"SignOn", "AddStringValue_0_1");

        lionWebServer.WaitForReceived(2);  

        var expected = new DataTypeTestConcept("a")
        {
            StringValue_0_1 = "new property"
        };

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Changes a property value for a server partition and verifies the change.
    /// </summary>
    [Test]
    public void ChangeProperty()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new DataTypeTestConcept("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", serverPartition.GetType().ToString(),"SignOn", "AddStringValue_0_1", "SetStringValue_0_1");

        lionWebServer.WaitForReceived(3);  

        var expected = new DataTypeTestConcept("a")
        {
            StringValue_0_1 = "changed property"
        };

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Deletes a property value from the server partition and verifies the deletion.
    /// </summary>
    [Test]
    public void DeleteProperty()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new DataTypeTestConcept("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", serverPartition.GetType().ToString(),"SignOn", "AddStringValue_0_1", "DeleteStringValue_0_1");

        lionWebServer.WaitForReceived(3);  

        var expected = new DataTypeTestConcept("a")
        {
            StringValue_0_1 = null
        };

        AssertEquals(expected, serverPartition);
    }
}
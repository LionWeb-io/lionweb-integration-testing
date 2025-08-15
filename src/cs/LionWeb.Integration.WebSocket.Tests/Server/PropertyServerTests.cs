using LionWeb.Core.M1;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Client;
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
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType(),Tasks.SignOn, Tasks.AddStringValue_0_1);

        WaitForSent(2);  

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
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType(),Tasks.SignOn, Tasks.AddStringValue_0_1, Tasks.SetStringValue_0_1);

        WaitForSent(3);  

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
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType(),Tasks.SignOn, Tasks.AddStringValue_0_1, Tasks.DeleteStringValue_0_1);

        WaitForSent(3);  

        var expected = new DataTypeTestConcept("a")
        {
            StringValue_0_1 = null
        };

        AssertEquals(expected, serverPartition);
    }
}
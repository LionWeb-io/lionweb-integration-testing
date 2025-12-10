using LionWeb.Core.M1;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Client;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Tests.Server;

public class PropertyServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds a new property to a server partition and verifies the change.
    /// </summary>
    [Test]
    public void AddProperty()
    {
        _webSocketServer = new TestWebSocketServer(_lionWebVersion, Port) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest,
            _webSocketServer.Connector);

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddStringValue_0_1);

        WaitForSent(3);

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data")
            {
                StringValue_0_1 = "new property"
            },
            Links =
            [
                new LinkTestConcept("link")
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.Last();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Changes a property value for a server partition and verifies the change.
    /// </summary>
    [Test]
    public void ChangeProperty()
    {
        _webSocketServer = new TestWebSocketServer(_lionWebVersion, Port) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest,
            _webSocketServer.Connector);

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddStringValue_0_1, Tasks.SetStringValue_0_1);

        WaitForSent(4);

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data")
            {
                StringValue_0_1 = "changed property"
            },
            Links =
            [
                new LinkTestConcept("link")
            ]
          
        };

        var serverPartition = (TestPartition)serverForest.Partitions.Last();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Deletes a property value from the server partition and verifies the deletion.
    /// </summary>
    [Test]
    public void DeleteProperty()
    {
        _webSocketServer = new TestWebSocketServer(_lionWebVersion, Port) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest,
            _webSocketServer.Connector);

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddStringValue_0_1,
            Tasks.DeleteStringValue_0_1);

        WaitForSent(4);

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data")
            {
                StringValue_0_1 = null
            },
            Links =
            [
                new LinkTestConcept("link")
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.Last();
        AssertEquals(expected, serverPartition);
    }
}
using LionWeb.Core.M1;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Client;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Tests.Server;

public class ReferenceServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds a reference to a specified child node
    /// </summary>
    [Test]
    public void AddReference()
    {
        _webSocketServer = new TestWebSocketServer(_lionWebVersion, Port) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer.Connector);

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1,
            Tasks.AddReference_0_1_to_Containment_0_1);

        WaitForSent(4);

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_1 = new LinkTestConcept("containment_0_1"),
                }
            ]
        };
        expected.Links[0].Reference_0_1 = expected.Links[0].Containment_0_1;

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Deletes a reference to a specified child node.
    /// </summary>
    [Test]
    public void DeleteReference()
    {
        _webSocketServer = new TestWebSocketServer(_lionWebVersion, Port) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer.Connector);

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1,
            Tasks.AddReference_0_1_to_Containment_0_1, Tasks.DeleteReference_0_1);

        WaitForSent(5);

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_1 = new LinkTestConcept("containment_0_1"),
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Changes the reference to a another child node
    /// </summary>
    [Test]
    public void ChangeReference()
    {
        _webSocketServer = new TestWebSocketServer(_lionWebVersion, Port) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer.Connector);

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1, Tasks.AddContainment_1,
            Tasks.AddReference_0_1_to_Containment_0_1, Tasks.AddReference_0_1_to_Containment_1);

        WaitForSent(6);

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                { 
                    Containment_0_1 = new LinkTestConcept("containment_0_1"),
                    Containment_1 = new LinkTestConcept("containment_1")
                }
            ]
           
        };
        expected.Links[0].Reference_0_1 = expected.Links[0].Containment_1;

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }
}
using LionWeb.Core.M1;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Client;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Tests.Server;

public class AnnotationServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds an annotation to the specified server partition.
    /// </summary>
    [Test]
    public void AddAnnotation()
    {
        _webSocketServer = new TestWebSocketServer(_lionWebVersion, Port) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer.Connector);

        StartClient("A", typeof(TestPartition), Tasks.SignOn, Tasks.AddPartition, Tasks.AddAnnotation);

        WaitForSent(3);

        var expected = new TestPartition("partition");
        expected.AddAnnotations([new TestAnnotation("annotation")]);

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Deletes an annotation associated with the specified server partition.
    /// </summary>
    [Test]
    public void DeleteAnnotation()
    {
        _webSocketServer = new TestWebSocketServer(_lionWebVersion, Port) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer.Connector);

        StartClient("A", typeof(TestPartition), Tasks.SignOn, Tasks.AddPartition, Tasks.AddAnnotation, Tasks.DeleteAnnotation);

        WaitForSent(4);

        var expected = new TestPartition("partition");
        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves an annotation from its current parent to another parent within the server partition.
    /// </summary>
    [Test]
    public void MoveAnnotationFromOtherParent()
    {
        _webSocketServer = new TestWebSocketServer(_lionWebVersion, Port) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer.Connector);

        StartClient("A", typeof(TestPartition), Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1,
            Tasks.AddAnnotation_to_Containment_0_1,
            Tasks.MoveAnnotationFromOtherParent);

        WaitForSent(5);

        var expected = new TestPartition("partition")
        {
            Links =
            [
                new LinkTestConcept("ltc")
                {
                    Containment_0_1 = new LinkTestConcept("containment_0_1")
                }
            ]
        };
        expected.AddAnnotations([new TestAnnotation("annotation")]);

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves an annotation within the same parent.
    /// </summary>
    [Test]
    public void MoveAnnotationInSameParent()
    {
        _webSocketServer = new TestWebSocketServer(_lionWebVersion, Port) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer.Connector);

        StartClient("A", typeof(TestPartition), Tasks.SignOn, Tasks.AddPartition, Tasks.AddAnnotations, Tasks.MoveAnnotationInSameParent);

        WaitForSent(5);

        var expected = new TestPartition("partition");
        expected.AddAnnotations([new TestAnnotation("annotation1"), new TestAnnotation("annotation0")]);

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }
}
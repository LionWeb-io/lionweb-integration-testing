using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Server;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Tests.Server;

[TestFixture(ClientProcesses.CSharp)]
public class AnnotationServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds an annotation to the specified server partition.
    /// </summary>
    [Test]
    public void AddAnnotation()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddAnnotation");

        lionWebServer.WaitForReceived(2);

        var expected = new LinkTestConcept("a");
        expected.AddAnnotations([new TestAnnotation("annotation")]);

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Deletes an annotation associated with the specified server partition.
    /// </summary>
    [Test]
    public void DeleteAnnotation()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddAnnotation", "DeleteAnnotation");

        lionWebServer.WaitForReceived(3);

        var expected = new LinkTestConcept("a");
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves an annotation from its current parent to another parent within the server partition.
    /// </summary>
    [Test]
    public void MoveAnnotationFromOtherParent()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_1", "AddAnnotation_to_Containment_0_1",
            "MoveAnnotationFromOtherParent");

        lionWebServer.WaitForReceived(4);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = new LinkTestConcept("containment_0_1")
        };
        expected.AddAnnotations([new TestAnnotation("annotation")]);

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves an annotation within the same parent.
    /// </summary>
    [Test]
    public void MoveAnnotationInSameParent()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddAnnotations", "MoveAnnotationInSameParent");

        lionWebServer.WaitForReceived(4);

        var expected = new LinkTestConcept("a");
        expected.AddAnnotations([new TestAnnotation("annotation1"), new TestAnnotation("annotation0")]);

        AssertEquals(expected, serverPartition);
    }
}
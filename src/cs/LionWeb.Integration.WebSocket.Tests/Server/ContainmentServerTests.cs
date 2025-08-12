using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Server;
using LionWeb.Protocol.Delta.Repository;
using NUnit.Framework.Legacy;

namespace LionWeb.Integration.WebSocket.Tests.Server;

[TestFixture(ClientProcesses.CSharp)]
public class ContainmentServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds a child node to the partition.
    /// </summary>
    [Test]
    public void AddChild()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_1");

        lionWebServer.WaitForReceived(2);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = new LinkTestConcept("containment_0_1")
        };

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Deletes a child node from the partition.
    /// </summary>
    [Test]
    public void DeleteChild()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_1", "DeleteContainment_0_1");

        lionWebServer.WaitForReceived(3);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = null
        };

        ClassicAssert.Null(serverPartition.Containment_0_1);
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Replaces the child node with a new one in the partition.
    /// </summary>
    [Test]
    public void ReplaceChild()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_1", "ReplaceContainment_0_1");

        lionWebServer.WaitForReceived(3);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = new LinkTestConcept("substitute")
        };

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves a child node from a single containment to another. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainment_Single()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_1", "AddContainment_0_1_Containment_0_1",
            "MoveChildFromOtherContainment_Single");

        lionWebServer.WaitForReceived(4);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = new LinkTestConcept("containment_0_1"),
            Containment_1 = new LinkTestConcept("containment_0_1_containment_0_1")
        };

        AssertEquals(expected, serverPartition);
    }
    
    /// <summary>
    /// Moves a child node from a multiple containment to another. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainment_Multiple()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_1_n", "AddContainment_0_n_Containment_0_n",
            "MoveChildFromOtherContainment_Multiple");

        lionWebServer.WaitForReceived(5);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_n = [new LinkTestConcept("containment_0_n_child0")],
            Containment_1_n = [new LinkTestConcept("containment_1_n_child0"), new LinkTestConcept("containment_0_n_containment_0_n_child0"), new LinkTestConcept("containment_1_n_child1")]
        };

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves and replaces a child node from a single containment to another. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildFromOtherContainment_Single()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_1", "AddContainment_0_1_Containment_0_1",
            "AddContainment_1", "AddContainment_1_Containment_0_1", "MoveAndReplaceChildFromOtherContainment_Single");

        lionWebServer.WaitForReceived(6);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = new LinkTestConcept("containment_0_1"),
            Containment_1 = new LinkTestConcept("containment_1")
            {
                Containment_0_1 = new LinkTestConcept("containment_0_1_containment_0_1")
            }
        };

        AssertEquals(expected, serverPartition);
    }
    
    /// <summary>
    /// Moves and replaces a child node from a multiple containment to another. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildFromOtherContainment_Multiple()
    {
        //Todo: MoveChildFromOtherContainment and DeleteChild commands are triggered
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_1_n", "AddContainment_0_n_Containment_0_n",
            "MoveAndReplaceChildFromOtherContainment_Multiple");

        lionWebServer.WaitForReceived(5);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_n = [new LinkTestConcept("containment_0_n_child0")],
            Containment_1_n = [new LinkTestConcept("containment_1_n_child0"), new LinkTestConcept("containment_0_n_containment_0_n_child0")]
        };

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves a child node from a single containment to another within the same parent node.
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainmentInSameParent_Single()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_1", "MoveChildFromOtherContainmentInSameParent_Single");

        lionWebServer.WaitForReceived(3);

        var expected = new LinkTestConcept("a")
        {
            Containment_1 = new LinkTestConcept("containment_0_1")
        };

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves and replaces a child node within the same parent containment.
    /// </summary>
    [Test]
    [Ignore("Fails to correlate notification id to ParticipationNotificationId")]
    public void MoveAndReplaceChildFromOtherContainmentInSameParent_Single()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_1", "AddContainment_1",
            "MoveAndReplaceChildFromOtherContainmentInSameParent_Single");

        lionWebServer.WaitForReceived(4);

        var expected = new LinkTestConcept("a")
        {
            Containment_1 = new LinkTestConcept("containment_0_1")
        };

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves a child node from a multiple containment to another within the same parent node.
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainmentInSameParent_Multiple()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_n", "AddContainment_1_n",
            "MoveChildFromOtherContainmentInSameParent_Multiple");

        lionWebServer.WaitForReceived(6);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_n = [new LinkTestConcept("containment_0_n_child0")],
            Containment_1_n = [new LinkTestConcept("containment_1_n_child0"), new LinkTestConcept("containment_0_n_child1"), new LinkTestConcept("containment_1_n_child1")]
        };

        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves a child node within the same containment.
    /// </summary>
    [Test]
    public void MoveChildInSameContainment()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        var serverForest = new Forest();
        serverForest.AddPartitions([serverPartition]);

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        StartClient("A", serverPartition.GetType().Name, "SignOn", "AddContainment_0_n",
            "MoveChildInSameContainment");

        lionWebServer.WaitForReceived(4);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_n = [new LinkTestConcept("containment_0_n_child1"), new LinkTestConcept("containment_0_n_child0")],
        };

        AssertEquals(expected, serverPartition);
    }
}
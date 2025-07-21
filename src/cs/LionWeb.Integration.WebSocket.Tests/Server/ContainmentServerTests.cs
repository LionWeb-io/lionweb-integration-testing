using System.Diagnostics;
using LionWeb.Core.M1.Event;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Server;
using LionWeb.Protocol.Delta.Repository;
using NUnit.Framework.Legacy;

namespace LionWeb.Integration.WebSocket.Tests.Server;

[TestFixture(ClientProcesses.CSharp)]
public class ContainmentServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds a new child node
    /// </summary>
    [Test]
    public void AddChild()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", "SignOn", "AddChild");

        lionWebServer.WaitForReceived(2);

        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = new LinkTestConcept("child")
        };

        AssertEquals(expected, serverPartition);
    }
      
    /// <summary>
    /// First adds and then deletes child node
    /// </summary>
    [Test]
    public void DeleteChild()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);
        
        StartClient("A", "SignOn", "AddChild", "DeleteChild");
        
        lionWebServer.WaitForReceived(3);
        
        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = null
        };

        ClassicAssert.Null(serverPartition.Containment_0_1);
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// First adds and then replaces child node
    /// </summary>
    [Test]
    public void ReplaceChild()
    { 
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new LinkTestConcept("a");
        
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);
        
        StartClient("A", "SignOn", "AddChild", "ReplaceChild");
        
        lionWebServer.WaitForReceived(3);
        
        var expected = new LinkTestConcept("a")
        {
            Containment_0_1 = new LinkTestConcept("substitute")
        };
        
        AssertEquals(expected, serverPartition);
    }
}
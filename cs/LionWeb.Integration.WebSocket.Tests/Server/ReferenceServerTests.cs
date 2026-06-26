using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Client;

namespace LionWeb.Integration.WebSocket.Tests.Server;

public class ReferenceServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds a reference to a specified child node
    /// </summary>
    [Test]
    public void AddReference()
    {
        var serverForest = CreateAndStartServer();

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
        var serverForest = CreateAndStartServer();

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
        var serverForest = CreateAndStartServer();

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
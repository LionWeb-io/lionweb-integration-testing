using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Client;

namespace LionWeb.Integration.WebSocket.Tests.Server;

public class PropertyServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds a new property to a server partition and verifies the change.
    /// </summary>
    [Test]
    public void AddProperty()
    {
        var serverForest = CreateAndStartServer();

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
        var serverForest = CreateAndStartServer();

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
        var serverForest = CreateAndStartServer();

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
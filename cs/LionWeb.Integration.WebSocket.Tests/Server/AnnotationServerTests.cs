using LionWeb.Core.M1;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Client;

namespace LionWeb.Integration.WebSocket.Tests.Server;

public class AnnotationServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds an annotation to the specified server partition.
    /// </summary>
    [Test]
    public void AddAnnotation()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddAnnotation);

        WaitForSent(3);

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
            ]
        };
        
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
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddAnnotation, Tasks.DeleteAnnotation);

        WaitForSent(4);

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
            ]
        };
        
        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves an annotation from its current parent to another parent within the server partition.
    /// </summary>
    [Test]
    public void MoveAnnotationFromOtherParent()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1,
            Tasks.AddAnnotation_to_Containment_0_1,
            Tasks.MoveAnnotationFromOtherParent);

        WaitForSent(5);

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
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
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddAnnotations, Tasks.MoveAnnotationInSameParent);

        WaitForSent(5);

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
            ]
        };
        expected.AddAnnotations([new TestAnnotation("annotation1"), new TestAnnotation("annotation0")]);

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }
    
    /// <summary>
    /// Adds an annotation with a reference to an M2 element.
    /// </summary>
    [Test]
    public void AddAnnotationWithLanguageReference()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddAnnotationWithLanguageReference);

        WaitForSent(3);

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
            ]
        }.WithAnnotation(new TestAnnotation("annotation")
        {
            Ref = TestLanguageLanguage.Instance.DataTypeTestConcept_booleanValue_0_1
        });

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

}
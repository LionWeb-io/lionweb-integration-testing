using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Client;
using NUnit.Framework.Legacy;

namespace LionWeb.Integration.WebSocket.Tests.Server;

public class ContainmentServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    /// <summary>
    /// Adds a child node to the partition.
    /// </summary>
    [Test]
    public void AddChild()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1);

        WaitForSent(3);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_1 = new LinkTestConcept("containment_0_1")
                }
            ]
        };

        AssertEquals(expected, (TestPartition)serverForest.Partitions.First());
    }

    /// <summary>
    /// Deletes a child node from the partition.
    /// </summary>
    [Test]
    public void DeleteChild()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1, Tasks.DeleteContainment_0_1);

        WaitForSent(4);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_1 = null
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        ClassicAssert.Null(serverPartition.Links[0].Containment_0_1);
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Replaces the child node with a new one in the partition.
    /// </summary>
    [Test]
    public void ReplaceChild()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1, Tasks.ReplaceContainment_0_1);

        WaitForSent(4);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_1 = new LinkTestConcept("substitute")
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    #region Move

    /// <summary>
    /// Moves a child node forward (positive offset) within the same containment.
    /// </summary>
    [Test]
    public void MoveChildInSameContainmentInSameParent_Forward()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_n,
         Tasks.AddContainment_0_n_Containment_0_n, Tasks.MoveChildInSameContainmentInSameParent_Forward);

        WaitForSent(6);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_n =
                    [
                        new LinkTestConcept("containment_0_n_child1"),
                        new LinkTestConcept("containment_0_n_child0_deep")
                        {
                            Containment_0_n = [new LinkTestConcept("containment_0_n_containment_0_n_child0")]
                        },
                        new LinkTestConcept("containment_0_n_child0"),
                    ]
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves a child node backward (negative offset) within the same containment.
    /// </summary>
    [Test]
    public void MoveChildInSameContainmentInSameParent_Backward()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_n, Tasks.AddContainment_0_n_Containment_0_n
            , Tasks.MoveChildInSameContainmentInSameParent_Backward
        );

        WaitForSent(6);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_n =
                    [
                        new LinkTestConcept("containment_0_n_child0_deep")
                        {
                            Containment_0_n = [new LinkTestConcept("containment_0_n_containment_0_n_child0")]
                        },
                        new LinkTestConcept("containment_0_n_child0"),
                        new LinkTestConcept("containment_0_n_child1"),
                    ],
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves a child node from a single containment to another within the same parent node.
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainmentInSameParent_Single()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1, Tasks.MoveChildFromOtherContainmentInSameParent_Single);

        WaitForSent(4);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_1 = new LinkTestConcept("containment_0_1")
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves a child node from a multiple containment to another within the same parent node.
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainmentInSameParent_Multiple()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_n, Tasks.AddContainment_1_n,
            Tasks.MoveChildFromOtherContainmentInSameParent_Multiple);

        WaitForSent(7);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_n = [new LinkTestConcept("containment_0_n_child0")],
                    Containment_1_n = [new LinkTestConcept("containment_1_n_child0"), new LinkTestConcept("containment_0_n_child1"), new LinkTestConcept("containment_1_n_child1")]
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves a child node from a single containment to another. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveChildFromContainmentInOtherParent_Single()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1, Tasks.AddContainment_0_1_Containment_0_1,
            Tasks.MoveChildFromContainmentInOtherParent_Single);

        WaitForSent(5);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_1 = new LinkTestConcept("containment_0_1"),
                    Containment_1 = new LinkTestConcept("containment_0_1_containment_0_1")
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }


    /// <summary>
    /// Moves a child node from a multiple containment to another. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveChildFromContainmentInOtherParent_Multiple()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_1_n, Tasks.AddContainment_0_n_Containment_0_n,
            Tasks.MoveChildFromContainmentInOtherParent_Multiple);

        WaitForSent(6);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_n = [new LinkTestConcept("containment_0_n_child0_deep")],
                    Containment_1_n = [new LinkTestConcept("containment_1_n_child0"), new LinkTestConcept("containment_0_n_containment_0_n_child0"), new LinkTestConcept("containment_1_n_child1")]
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    #endregion

    #region Move and replace

    /// <summary>
    /// Moves and replaces a child node forward (positive offset) within the same containment.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildInSameContainmentInSameParent_Forward()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_n, Tasks.AddContainment_0_n_Containment_0_n,
            Tasks.MoveAndReplaceChildInSameContainmentInSameParent_Forward);

        WaitForSent(6);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_n =
                    [
                        new LinkTestConcept("containment_0_n_child1"),
                        new LinkTestConcept("containment_0_n_child0"),
                    ]
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves and replaces a child node backward (negative offset) within the same containment.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildInSameContainmentInSameParent_Backward()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_n, Tasks.AddContainment_0_n_Containment_0_n,
            Tasks.MoveAndReplaceChildInSameContainmentInSameParent_Backward);

        WaitForSent(6);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_n =
                    [
                        new LinkTestConcept("containment_0_n_child0_deep")
                        {
                            Containment_0_n = [new LinkTestConcept("containment_0_n_containment_0_n_child0")]
                        },
                        new LinkTestConcept("containment_0_n_child1"),
                    ],
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves and replaces a child node within the same parent containment.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildFromOtherContainmentInSameParent_Single()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1, Tasks.AddContainment_1,
            Tasks.MoveAndReplaceChildFromOtherContainmentInSameParent_Single);

        WaitForSent(5);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_1 = new LinkTestConcept("containment_0_1")
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves a child node from a multiple containment to another within the same parent node.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildFromOtherContainmentInSameParent_Multiple()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_n, Tasks.AddContainment_1_n,
            Tasks.MoveAndReplaceChildFromOtherContainmentInSameParent_Multiple);

        WaitForSent(7);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_n = [new LinkTestConcept("containment_0_n_child0")],
                    Containment_1_n = [new LinkTestConcept("containment_1_n_child0"), new LinkTestConcept("containment_0_n_child1")]
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves and replaces a child node from a single containment to another. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildFromContainmentInOtherParent_Single()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1, Tasks.AddContainment_0_1_Containment_0_1,
            Tasks.AddContainment_1, Tasks.AddContainment_1_Containment_0_1, Tasks.MoveAndReplaceChildFromContainmentInOtherParent_Single);

        WaitForSent(7);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_1 = new LinkTestConcept("containment_0_1"),
                    Containment_1 = new LinkTestConcept("containment_1")
                    {
                        Containment_0_1 = new LinkTestConcept("containment_0_1_containment_0_1")
                    }
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    /// <summary>
    /// Moves and replaces a child node from a multiple containment to another. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildFromContainmentInOtherParent_Multiple()
    {
        var serverForest = CreateAndStartServer();

        StartClient("A", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_1_n, Tasks.AddContainment_0_n_Containment_0_n, Tasks.MoveAndReplaceChildFromContainmentInOtherParent_Multiple);

        WaitForSent(6);

        var expected = new TestPartition("partition")
        {
            Name = "my test partition",
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_n = [new LinkTestConcept("containment_0_n_child0_deep")],
                    Containment_1_n = [new LinkTestConcept("containment_1_n_child0"), new LinkTestConcept("containment_0_n_containment_0_n_child0")]
                }
            ]
        };

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        AssertEquals(expected, serverPartition);
    }

    #endregion
}
// Copyright 2024 TRUMPF Laser GmbH
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// SPDX-FileCopyrightText: 2024 TRUMPF Laser GmbH
// SPDX-License-Identifier: Apache-2.0

using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;

namespace LionWeb.Integration.WebSocket.Tests.Client;

[TestClass]
public class ContainmentClientTests : LinkClientTestBase
{
    /// <summary>
    /// Added child is a single node
    /// </summary>
    [TestMethod]
    public void AddChild() => Timeout(() =>
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    });
    
    /// <summary>
    /// Added child is a (complex) subtree
    /// </summary>
    [TestMethod]
    public void AddChild_AsASubtree() => Timeout(() =>
    {
        var subTree = new LinkTestConcept("subtree")
        {
            Name = "containment_0_1",
            Containment_0_1 = new LinkTestConcept("containment_0_1"),
            Containment_0_n = new List<LinkTestConcept> { new("child1"), new("child2") }
        };

        aPartition.Containment_0_1 = subTree;
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    });


    /// <summary>
    /// Node in an added subtree has a reference to already existing node
    /// </summary>
    [TestMethod]
    public void AddChild_NodeInAddedSubtreeHasAReferenceToAlreadyExistingNodes() => Timeout(() =>
    {
        aPartition.Containment_1 = new LinkTestConcept("referenced-child");
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = new LinkTestConcept("subtree")
        {
            Reference_1 = bPartition.Containment_1
        };
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    });

    /// <summary>
    /// Already existing node has a reference to a node in an added subtree
    /// </summary>
    [TestMethod]
    public void AddChild_AlreadyExistingNodeHasAReferenceToNodeInAddedSubtree() => Timeout(() =>
    {
        aPartition.Containment_0_1 = new LinkTestConcept("existing-subtree")
        {
            Containment_0_1 = new LinkTestConcept("containment")
        };
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);

        aPartition.Containment_1 = new LinkTestConcept("added-node");
        bClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);
        
        // This works, but reference link is established after the nodes are added.
        // One would expect reference from a child is set to the node before the new node
        // is added to partition!? 
        aPartition.Containment_0_1.Reference_0_1 = aPartition.Containment_1;
        bClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);

    });

    
    /// <summary>
    /// Deletes an existing node
    /// </summary>
    [TestMethod]
    public void DeleteChild() => Timeout(() =>
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_0_1 = null;
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    });
    
    /// <summary>
    /// Deletes a node from an existing subtree
    /// </summary>
    [TestMethod]
    public void DeleteChild_FromASubtree() => Timeout(() =>
    {
        aPartition.Containment_1 = new LinkTestConcept("child")
        {
            Containment_0_1 = new LinkTestConcept("deleted-node")
        };
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1.Containment_0_1 = null;
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    });

    /// <summary>
    /// Replaces an existing node with a new node
    /// </summary>
    [TestMethod]
    public void ReplaceChild() => Timeout(() =>
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_0_1 = new LinkTestConcept("replacedChild") { Name = "replaced" };
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    });

    /// <summary>
    /// Replaces an existing node a (complex) subtree
    /// </summary>
    [TestMethod]
    public void ReplaceChild_WithASubtree() => Timeout(() =>
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_0_1 = new LinkTestConcept("subtree")
        {
            Name = "containment_0_1",
            Containment_0_1 = new LinkTestConcept("containment_0_1"),
            Containment_0_n = new List<LinkTestConcept> { new("child1"), new("child2") }
        };
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    });

    /// <summary>
    /// Moves a child to a new containment which has another parent.
    /// </summary>
    [TestMethod]
    public void MoveChildFromOtherContainment() => Timeout(() =>
    {
        aPartition.Containment_0_1 = new LinkTestConcept("subHost") { Containment_0_1 = new LinkTestConcept("child") };
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = bPartition.Containment_0_1!.Containment_0_1!;
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    });

    
    /// <summary>
    /// Moves a child to a new containment which has another parent; replaces the existing child.
    /// </summary>
    [TestMethod]
    public void MoveAndReplaceChildFromOtherContainment() => Timeout(() =>
    {
        aPartition.Containment_0_1 =  new LinkTestConcept("moved-subHost") { Containment_0_1 = new LinkTestConcept("moved-child") };
        bClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = new LinkTestConcept("replaced-subHost"){ Containment_0_1 = new LinkTestConcept("replaced-child")};
        aClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1.Containment_0_1 = bPartition.Containment_0_1!.Containment_0_1!;
        aClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);
    });


    /// <summary>
    /// Moves a child from one containment to another within the same parent. 
    /// </summary>
    [TestMethod]
    public void MoveChildFromOtherContainmentInSameParent() => Timeout(() =>
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = bPartition.Containment_0_1!;
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    });

    /// <summary>
    /// Moves a child from one containment to another within the same parent and replaces the existing child node, if any.
    /// </summary>
    [TestMethod]
    public void MoveAndReplaceChildFromOtherContainmentInSameParent() => Timeout(() =>
    {
        aPartition.Containment_0_1 = new LinkTestConcept("moved-child");
        bClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = new LinkTestConcept("replaced-child");
        aClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = bPartition.Containment_0_1!;
        aClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);
    });

    /// <summary>
    /// Moves child within the same containment to a new index
    /// </summary>
    [TestMethod]
    public void MoveChildInSameContainment() => Timeout(() =>
    {
        aPartition.AddContainment_0_n([new LinkTestConcept("child0"), new LinkTestConcept("child1")]);
        bClient.WaitForReplies(2);

        AssertEquals(aPartition, bPartition);

        bPartition.InsertContainment_0_n(0, [bPartition.Containment_0_n.Last()]);
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    });
}
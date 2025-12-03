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

using LionWeb.Core.M1;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;

namespace LionWeb.Integration.WebSocket.Tests.Client;

public class ContainmentClientTests(ServerProcesses serverProcess) : LinkClientTestBase(serverProcess)
{
    /// <summary>
    /// Added child is a single node
    /// </summary>
    [Test]
    public void AddChild()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
    }
    
    /// <summary>
    /// Added child is a (complex) subtree
    /// </summary>
    [Test]
    public void AddChild_AsASubtree()
    {
        var subTree = new LinkTestConcept("subtree")
        {
            Name = "containment_0_1",
            Containment_0_1 = new LinkTestConcept("containment_0_1"),
            Containment_0_n = new List<LinkTestConcept> { new("child1"), new("child2") }
        };

        aPartition.Containment_0_1 = subTree;
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
    }

    /// <summary>
    /// Node in an added subtree has a reference to already existing node
    /// </summary>
    [Test]
    [Ignore("Requires M1Extension.ReplaceWith() to handle notifications")]
    public void AddChild_NodeInAddedSubtreeHasAReferenceToAlreadyExistingNodes()
    {
        aPartition.Containment_1 = new LinkTestConcept("referenced-child");
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = new LinkTestConcept("subtree")
        {
            Reference_1 = bPartition.Containment_1
        };
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
    }
    
    /// <summary>
    /// Already existing node has a reference to a node in an added subtree
    /// </summary>
    [Test]
    public void AddChild_AlreadyExistingNodeHasAReferenceToNodeInAddedSubtree()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("existing-subtree")
        {
            Containment_0_1 = new LinkTestConcept("containment")
        };
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        aPartition.Containment_1 = new LinkTestConcept("added-node");
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);
        
        aPartition.Containment_0_1.Reference_0_1 = aPartition.Containment_1;
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);
    }

    
    /// <summary>
    /// Deletes an existing node
    /// </summary>
    [Test]
    public void DeleteChild()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_0_1 = null;
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
    }
    
    /// <summary>
    /// Deletes a node from an existing subtree
    /// </summary>
    [Test]
    public void DeleteChild_FromASubtree()
    {
        aPartition.Containment_1 = new LinkTestConcept("child")
        {
            Containment_0_1 = new LinkTestConcept("deleted-node")
        };
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1.Containment_0_1 = null;
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
    }


    /// <summary>
    /// Replaces an existing node with a new node
    /// </summary>
    [Test]
    [Ignore("Requires M1Extension.ReplaceWith() to handle notifications")]
    public void ReplaceChild()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_0_1 = new LinkTestConcept("replacedChild") { Name = "replaced" };
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
    }
    
    /// <summary>
    /// Replaces an existing node a (complex) subtree
    /// </summary>
    [Test]
    [Ignore("Requires M1Extension.ReplaceWith() to handle notifications")]
    public void ReplaceChild_WithASubtree()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_0_1 = new LinkTestConcept("subtree")
        {
            Name = "containment_0_1",
            Containment_0_1 = new LinkTestConcept("containment_0_1"),
            Containment_0_n = new List<LinkTestConcept> { new("child1"), new("child2") }
        };
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
    }

    /// <summary>
    /// Move a child node from a single containment to another. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainment_Single()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("subHost") { Containment_0_1 = new LinkTestConcept("child") };
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = bPartition.Containment_0_1!.Containment_0_1!;
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
    }
    
    /// <summary>
    /// Move a child node from a multiple containment to another.  Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainment_Multiple()
    {
        aPartition.AddContainment_0_n([new LinkTestConcept("child0") { Containment_0_n = [new LinkTestConcept("moved")] }]);

        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);

        aPartition.AddContainment_1_n([new LinkTestConcept("child1"), new LinkTestConcept("child2")]);
        WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        bPartition.InsertContainment_1_n(0, [bPartition.Containment_0_n[^1].Containment_0_n[0]]);
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
    }
    
    /// <summary>
    /// Moves a child from a single containment to other single containment (which has another parent) and replaces the existing child.
    /// </summary>
    [Test]
    [Ignore("Requires M1Extension.ReplaceWith() to handle notifications")]
    public void MoveAndReplaceChildFromOtherContainment_Single_WithAssignment()
    {
        aPartition.Containment_0_1 =  new LinkTestConcept("moved-subHost") { Containment_0_1 = new LinkTestConcept("moved-child") };
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = new LinkTestConcept("replaced-subHost"){ Containment_0_1 = new LinkTestConcept("replaced-child")};
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1.Containment_0_1 = bPartition.Containment_0_1!.Containment_0_1!;
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);
    }
    
    /// <summary>
    /// Moves a child from a single containment to other single containment (which has another parent) and replaces the existing child.
    /// </summary>
    [Test]
    [Ignore("Requires M1Extension.ReplaceWith() to handle notifications")]
    public void MoveAndReplaceChildFromOtherContainment_Single_WithReplaceWith()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("moved-subHost") { Containment_0_1 = new LinkTestConcept("moved-child") };
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = new LinkTestConcept("replaced-subHost") { Containment_0_1 = new LinkTestConcept("replaced-child") };
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1.Containment_0_1.ReplaceWith(bPartition.Containment_0_1!.Containment_0_1!);
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
    }


    /// <summary>
    /// Moves a child from a multiple containment to other multiple containment within the same parent and replaces the existing child.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildFromOtherContainmentInSameParent_Multiple()
    {
        aPartition.AddContainment_0_n([new LinkTestConcept("child0"), new LinkTestConcept("moved")]);
        WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        bPartition.AddContainment_1_n([new LinkTestConcept("child1"), new LinkTestConcept("replaced")]);
        WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        aPartition.Containment_1_n[^1].ReplaceWith(aPartition.Containment_0_n[^1]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
    }

    /// <summary>
    /// Moves a child from a multiple containment to other multiple containment and replaces the existing child. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildFromOtherContainment_Multiple()
    {
        //todo: MoveChildFromOtherContainment and DeleteChild commands are triggered
        aPartition.AddContainment_0_n([new LinkTestConcept("child0"), new LinkTestConcept("moved")]);
        WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        bPartition.AddContainment_1_n([new LinkTestConcept("child1"), new LinkTestConcept("child2")]);
        WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1_n[^1].AddContainment_0_n([new LinkTestConcept("child3"), new LinkTestConcept("replaced")]);
        WaitForReceived(2);
        
        AssertEquals(aPartition, bPartition);

        aPartition.Containment_1_n[^1].Containment_0_n[^1].ReplaceWith(aPartition.Containment_0_n[^1]);
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
    }

    /// <summary>
    /// Moves a child from one containment to another within the same parent. 
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainmentInSameParent_Single()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = bPartition.Containment_0_1;
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
    }
    
    /// <summary>
    /// Moves a child from one containment to another within the same parent and replaces the existing child node.
    /// </summary>
    [Test]
    [Ignore("Fails to correlate notification id to ParticipationNotificationId")]
    public void MoveAndReplaceChildFromOtherContainmentInSameParent_Single()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("moved-child");
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = new LinkTestConcept("replaced-child");
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);

        bPartition.Containment_1 = bPartition.Containment_0_1!;
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);
    }

    /// <summary>
    /// Moves child within the same containment to a new index
    /// </summary>
    [Test]
    public void MoveChildInSameContainment()
    {
        aPartition.AddContainment_0_n([new LinkTestConcept("child0"), new LinkTestConcept("child1")]);
        WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        bPartition.InsertContainment_0_n(0, [bPartition.Containment_0_n.Last()]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
    }
}
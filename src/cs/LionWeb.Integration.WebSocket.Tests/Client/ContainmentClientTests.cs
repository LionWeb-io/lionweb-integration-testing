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
using NUnit.Framework.Legacy;

namespace LionWeb.Integration.WebSocket.Tests.Client;

public class ContainmentClientTests(ServerProcesses serverProcess) : LinkClientTestBase(serverProcess)
{
    /// <summary>
    /// Added child is a single node
    /// </summary>
    [Test]
    public void AddChild()
    {
        aPartition.AddLinks([new LinkTestConcept("child")]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
        Assert.That(bPartition.Links[0].GetId(), Is.EqualTo("child"));
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

        aPartition.AddLinks([subTree]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
        Assert.That(bPartition.Links[0].GetId(), Is.EqualTo("subtree"));
    }

    /// <summary>
    /// Node in an added subtree has a reference to already existing node
    /// </summary>
    [Test]
    [Ignore("Requires M1Extension.ReplaceWith() to handle notifications")]
    public void AddChild_NodeInAddedSubtreeHasAReferenceToAlreadyExistingNodes()
    {
        aPartition.AddLinks([new LinkTestConcept("referenced-child")]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.AddLinks([new LinkTestConcept("subtree")
        {
            Reference_1 = bPartition.Links[0]
        }]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].Reference_1.GetId(), Is.EqualTo("referenced-child"));
    }
    
    /// <summary>
    /// Already existing node has a reference to a node in an added subtree
    /// </summary>
    [Test]
    public void AddChild_AlreadyExistingNodeHasAReferenceToNodeInAddedSubtree()
    {
        aPartition.AddLinks([new LinkTestConcept("existing-subtree")
        {
            Containment_0_1 = new LinkTestConcept("containment")
        }]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        aPartition.AddLinks([new LinkTestConcept("added-node")]);
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);
        
        aPartition.Links[0].Reference_0_1 = aPartition.Links[1];
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);
        Assert.That(bPartition.Links[0].Reference_0_1!.GetId(), Is.EqualTo("added-node"));
    }

    
    /// <summary>
    /// Deletes an existing node
    /// </summary>
    [Test]
    public void DeleteChild()
    {
        aPartition.AddLinks([new LinkTestConcept("child")]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.RemoveLinks([bPartition.Links[0]]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links, Is.Empty);
    }
    
    /// <summary>
    /// Deletes a node from an existing subtree
    /// </summary>
    [Test]
    public void DeleteChild_FromASubtree()
    {
        aPartition.AddLinks([new LinkTestConcept("child")
        {
            Containment_0_1 = new LinkTestConcept("deleted-node")
        }]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].Containment_0_1 = null;
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].Containment_0_1, Is.Null);
    }


    /// <summary>
    /// Replaces an existing node with a new node
    /// </summary>
    [Test]
    [Ignore("Requires M1Extension.ReplaceWith() to handle notifications")]
    public void ReplaceChild()
    {
        aPartition.AddLinks([new LinkTestConcept("child")]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].ReplaceWith(new LinkTestConcept("replacedChild") { Name = "replaced" });
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].GetId(), Is.EqualTo("replacedChild"));
    }
    
    /// <summary>
    /// Replaces an existing node a (complex) subtree
    /// </summary>
    [Test]
    [Ignore("Requires M1Extension.ReplaceWith() to handle notifications")]
    public void ReplaceChild_WithASubtree()
    {
        aPartition.AddLinks([new LinkTestConcept("child")]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].ReplaceWith(new LinkTestConcept("subtree")
        {
            Name = "containment_0_1",
            Containment_0_1 = new LinkTestConcept("containment_0_1"),
            Containment_0_n = new List<LinkTestConcept> { new("child1"), new("child2") }
        });
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].GetId(), Is.EqualTo("subtree"));
    }

    /// <summary>
    /// Move a child node from a single containment to another. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainment_Single()
    {
        aPartition.AddLinks([new LinkTestConcept("parent") { Containment_0_1 = new LinkTestConcept("subHost") { Containment_0_1 = new LinkTestConcept("child") }}]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].Containment_1 = bPartition.Links[0].Containment_0_1!.Containment_0_1!;
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].Containment_1.GetId(), Is.EqualTo("child"));
    }
    
    /// <summary>
    /// Move a child node from a multiple containment to another.  Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainment_Multiple()
    {
        aPartition.AddLinks([new LinkTestConcept("parent"){ Containment_0_n = [new LinkTestConcept("child0") { Containment_0_n = [new LinkTestConcept("moved")] }]}]);

        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);

        aPartition.Links[0].AddContainment_1_n([new LinkTestConcept("child1"), new LinkTestConcept("child2")]);
        WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].InsertContainment_1_n(0, [bPartition.Links[0].Containment_0_n[^1].Containment_0_n[0]]);
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].Containment_1_n[0].GetId(), Is.EqualTo("moved"));
    }
    
    /// <summary>
    /// Moves a child from a single containment to other single containment (which has another parent) and replaces the existing child.
    /// </summary>
    [Test]
    [Ignore("Requires M1Extension.ReplaceWith() to handle notifications")]
    public void MoveAndReplaceChildFromOtherContainment_Single_WithAssignment()
    {
        aPartition.AddLinks([new LinkTestConcept("parent"){ Containment_0_1 =  new LinkTestConcept("moved-subHost") { Containment_0_1 = new LinkTestConcept("moved-child") }}]);
        WaitForReceived(2);
        
        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].Containment_1 = new LinkTestConcept("replaced-subHost"){ Containment_0_1 = new LinkTestConcept("replaced-child")};
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].Containment_1.Containment_0_1 = bPartition.Links[0].Containment_0_1!.Containment_0_1!;
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].Containment_1.Containment_0_1!.GetId(), Is.EqualTo("moved-child"));
    }
    
    /// <summary>
    /// Moves a child from a single containment to other single containment (which has another parent) and replaces the existing child.
    /// </summary>
    [Test]
    [Ignore("Requires M1Extension.ReplaceWith() to handle notifications")]
    public void MoveAndReplaceChildFromOtherContainment_Single_WithReplaceWith()
    {
        aPartition.AddLinks([new LinkTestConcept("parent"){ Containment_0_1 = new LinkTestConcept("moved-subHost") { Containment_0_1 = new LinkTestConcept("moved-child") }}]);
        WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].Containment_1 = new LinkTestConcept("replaced-subHost") { Containment_0_1 = new LinkTestConcept("replaced-child") };
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].Containment_1.Containment_0_1!.ReplaceWith(bPartition.Links[0].Containment_0_1!.Containment_0_1!);
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].Containment_1.Containment_0_1!.GetId(), Is.EqualTo("moved-child"));
    }


    /// <summary>
    /// Moves a child from a multiple containment to other multiple containment within the same parent and replaces the existing child.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildFromOtherContainmentInSameParent_Multiple()
    {
        aPartition.AddLinks([new LinkTestConcept("parent"){ Containment_0_n =[new LinkTestConcept("child0"), new LinkTestConcept("moved")]}]);
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].AddContainment_1_n([new LinkTestConcept("child1"), new LinkTestConcept("replaced")]);
        WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        aPartition.Links[0].Containment_1_n[^1].ReplaceWith(aPartition.Links[0].Containment_0_n[^1]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].Containment_1_n[1].GetId(), Is.EqualTo("moved"));
    }

    /// <summary>
    /// Moves a child from a multiple containment to other multiple containment and replaces the existing child. Both containments have different parents.
    /// </summary>
    [Test]
    public void MoveAndReplaceChildFromOtherContainment_Multiple()
    {
        //todo: MoveChildFromOtherContainment and DeleteChild commands are triggered
        aPartition.AddLinks([new LinkTestConcept("parent"){ Containment_0_n = [new LinkTestConcept("child0"), new LinkTestConcept("moved")]}]);
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].AddContainment_1_n([new LinkTestConcept("child1"), new LinkTestConcept("child2")]);
        WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].Containment_1_n[^1].AddContainment_0_n([new LinkTestConcept("child3"), new LinkTestConcept("replaced")]);
        WaitForReceived(2);
        
        AssertEquals(aPartition, bPartition);

        aPartition.Links[0].Containment_1_n[^1].Containment_0_n[^1].ReplaceWith(aPartition.Links[0].Containment_0_n[^1]);
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].Containment_1_n[1].Containment_0_n[1].GetId(), Is.EqualTo("moved"));
    }

    /// <summary>
    /// Moves a child from one containment to another within the same parent. 
    /// </summary>
    [Test]
    public void MoveChildFromOtherContainmentInSameParent_Single()
    {
        aPartition.AddLinks([new LinkTestConcept("parent"){ Containment_0_1 = new LinkTestConcept("child")}]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].Containment_1 = bPartition.Links[0].Containment_0_1!;
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].Containment_1.GetId(), Is.EqualTo("child"));
    }
    
    /// <summary>
    /// Moves a child from one containment to another within the same parent and replaces the existing child node.
    /// </summary>
    [Test]
    [Ignore("Fails to correlate notification id to ParticipationNotificationId")]
    public void MoveAndReplaceChildFromOtherContainmentInSameParent_Single()
    {
        aPartition.AddLinks([new LinkTestConcept("parent"){ Containment_0_1 = new LinkTestConcept("moved-child")}]);
        WaitForReceived(2);
        
        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].Containment_1 = new LinkTestConcept("replaced-child");
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].Containment_1 = bPartition.Links[0].Containment_0_1!;
        WaitForReceived();
        
        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].Containment_1.GetId(), Is.EqualTo("moved-child"));
    }

    /// <summary>
    /// Moves child within the same containment to a new index
    /// </summary>
    [Test]
    public void MoveChildInSameContainment()
    {
        aPartition.AddLinks([new LinkTestConcept("parent"){ Containment_0_n=[new LinkTestConcept("child0"), new LinkTestConcept("child1")]}]);
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);

        bPartition.Links[0].InsertContainment_0_n(0, [bPartition.Links[0].Containment_0_n.Last()]);
        WaitForReceived();

        AssertEquals(aPartition, bPartition);
        Assert.That(aPartition.Links[0].Containment_0_n[0].GetId(), Is.EqualTo("child1"));
    }
}
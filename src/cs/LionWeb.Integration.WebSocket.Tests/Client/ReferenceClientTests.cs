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

public class ReferenceClientTests(ServerProcesses serverProcess) : LinkClientTestBase(serverProcess)
{
    [Test]
    public void AddReference()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        aPartition.Reference_0_1 = aPartition.Containment_0_1;
        bClient.WaitForReplies(2);

        AssertEquals(aPartition, bPartition);
    }

    [Test]
    public void DeleteReference()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        aPartition.Reference_0_1 = aPartition.Containment_0_1;
        bClient.WaitForReplies(2);

        AssertEquals(aPartition, bPartition);

        bPartition.Reference_0_1 = null;
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    }

    [Test]
    public void ChangeReference()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("childA");
        aPartition.Containment_1 = new LinkTestConcept("childB");
        aPartition.Reference_0_1 = aPartition.Containment_0_1;
        bClient.WaitForReplies(3);

        AssertEquals(aPartition, bPartition);

        bPartition.Reference_0_1 = bPartition.Containment_0_1;
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    }

    [Test]
    public void MoveEntryFromOtherReference()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("subHost") { Containment_0_1 = new LinkTestConcept("child") };
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);

        Assert.Fail("no way to move reference");
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    }

    [Test]
    public void MoveEntryFromOtherReferenceInSameParent()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("child");
        bClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);

        Assert.Fail("no way to move reference");
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    }

    [Test]
    public void MoveEntryInSameReference()
    {
        aPartition.AddContainment_0_n([new LinkTestConcept("child0"), new LinkTestConcept("child1")]);
        bClient.WaitForReplies(2);

        AssertEquals(aPartition, bPartition);

        Assert.Fail("no way to move reference");
        aClient.WaitForReplies(1);

        AssertEquals(aPartition, bPartition);
    }
}
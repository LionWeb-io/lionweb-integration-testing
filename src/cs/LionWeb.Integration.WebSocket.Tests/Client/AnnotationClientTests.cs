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

public class AnnotationClientTests(ServerProcesses serverProcess) : LinkClientTestBase(serverProcess)
{
    [Test]
    public void AddAnnotation()
    {
        aPartition.AddAnnotations([new TestAnnotation("annotation")]);
        bClient.WaitForReceived(2);

        AssertEquals(aPartition, bPartition);
    }

    [Test]
    public void DeleteAnnotation()
    {
        aPartition.AddAnnotations([new TestAnnotation("annotation")]);
        bClient.WaitForReceived(1);

        AssertEquals(aPartition, bPartition);

        bPartition.RemoveAnnotations(bPartition.GetAnnotations());
        aClient.WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
    }

    [Test]
    public void ReplaceAnnotation()
    {
        aPartition.AddAnnotations([new TestAnnotation("annotation")]);
        bClient.WaitForReceived(1);

        AssertEquals(aPartition, bPartition);

        Assert.Fail("no way to replace annotation");
//        bPartition.annContainment_0_1 = new LinkTestConcept("replacedChild") { Name = "replaced" };
        aClient.WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
    }

    [Test]
    public void MoveAnnotationFromOtherParent()
    {
        aPartition.Containment_0_1 = new LinkTestConcept("subHost");
        aPartition.Containment_0_1.AddAnnotations([new TestAnnotation("annotation")]);
        bClient.WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        bPartition.AddAnnotations(bPartition.Containment_0_1.GetAnnotations());
        aClient.WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
    }

    [Test]
    public void MoveAnnotationInSameParent()

    {
        aPartition.AddAnnotations([new TestAnnotation("annotation0"), new TestAnnotation("annotation1")]);
        bClient.WaitForReceived(2);

        AssertEquals(aPartition, bPartition);

        bPartition.InsertAnnotations(0, [bPartition.GetAnnotations().Last()]);
        aClient.WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
    }
}
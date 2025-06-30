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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LionWeb.Integration.WebSocket.Tests.Client;

[TestClass]
public class ContainmentClientTests : LinkClientTestBase
{
    [TestMethod, Timeout(Timeout)]
    public void AddChild()
    {
        aPartition.Containment_0_1 = new DataTypeTestConcept("child");
        bClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);
    }
}
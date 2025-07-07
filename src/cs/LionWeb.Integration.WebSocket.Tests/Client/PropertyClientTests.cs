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

using LionWeb.Core;
using LionWeb.Integration.Languages;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Protocol.Delta.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LionWeb.Integration.WebSocket.Tests.Client;

[TestClass]
public class PropertyClientTests : WebSocketClientTestBase
{
    private readonly DataTypeTestConcept aPartition;
    private readonly LionWebTestClient aClient;

    private readonly DataTypeTestConcept bPartition;
    private readonly LionWebTestClient bClient;

    public PropertyClientTests() : base(LionWebVersions.v2023_1, [TestLanguageLanguage.Instance])
    {
        aPartition = new("partition");
        aClient = ConnectWebSocket(aPartition, "A").Result;

        bPartition = new("partition");
        bClient = ConnectWebSocket(bPartition, "B").Result;
    }

    /// <inheritdoc />
    protected override string AdditionalServerParameters() =>
        TestLanguageLanguage.Instance.DataTypeTestConcept.Key;
    
    [TestMethod, Timeout(TestTimeout)]
    public void AddProperty()
    {
        aPartition.StringValue_0_1 = "new property";
        bClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);
    }
    
    [TestMethod, Timeout(TestTimeout)]
    public void ChangeProperty()
    {
        aPartition.StringValue_0_1 = "new property";
        bClient.WaitForReplies(1);
        
        bPartition.StringValue_0_1 = "changed property";
        aClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);
    }
    
    [TestMethod, Timeout(TestTimeout)]
    public void DeleteProperty()
    {
        aPartition.StringValue_0_1 = "new property";
        bClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);
        
        bPartition.StringValue_0_1 = null;
        aClient.WaitForReplies(1);
        
        AssertEquals(aPartition, bPartition);
    }
}
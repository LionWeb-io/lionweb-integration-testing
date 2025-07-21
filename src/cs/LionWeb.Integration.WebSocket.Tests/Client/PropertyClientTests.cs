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
using LionWeb.Core.M1;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Protocol.Delta.Client;

namespace LionWeb.Integration.WebSocket.Tests.Client;

public class PropertyClientTests(ServerProcesses serverProcess)
    : WebSocketClientTestBase(serverProcess, LionWebVersions.v2023_1, [TestLanguageLanguage.Instance])
{
    private IForest aForest;
    private DataTypeTestConcept aPartition;
    private LionWebTestClient aClient;

    private IForest bForest;
    private DataTypeTestConcept bPartition;
    private LionWebTestClient bClient;

    [SetUp]
    public void ConnectToServer()
    {
        aForest = new Forest();
        aClient = ConnectWebSocket(aForest, "A").Result;

        bForest = new Forest();
        bClient = ConnectWebSocket(bForest, "B").Result;

        aPartition = new("partition");
        aForest.AddPartitions([aPartition]);
        bClient.WaitForReceived(1);
        bPartition = (DataTypeTestConcept)aForest.Partitions.First();
    }

    /// <inheritdoc />
    protected override string AdditionalServerParameters() =>
        TestLanguageLanguage.Instance.DataTypeTestConcept.Key;

    [Test]
    public void AddProperty()
    {
        aPartition.StringValue_0_1 = "new property";
        bClient.WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
    }

    [Test]
    public void ChangeProperty()
    {
        aPartition.StringValue_0_1 = "new property";
        bClient.WaitForReceived(1);

        bPartition.StringValue_0_1 = "changed property";
        aClient.WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
    }

    [Test]
    public void DeleteProperty()
    {
        aPartition.StringValue_0_1 = "new property";
        bClient.WaitForReceived(1);

        AssertEquals(aPartition, bPartition);

        bPartition.StringValue_0_1 = null;
        aClient.WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
    }
}
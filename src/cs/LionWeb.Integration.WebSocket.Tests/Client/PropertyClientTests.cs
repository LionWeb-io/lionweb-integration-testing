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

namespace LionWeb.Integration.WebSocket.Tests.Client;

public class PropertyClientTests(ServerProcesses serverProcess)
    : WebSocketClientTestBase(serverProcess, LionWebVersions.v2023_1, [TestLanguageLanguage.Instance])
{
    private DataTypeTestConcept aParent = null!;

    private DataTypeTestConcept bParent = null!;

    [SetUp]
    public void ConnectToServer()
    {
        aForest = new Forest();
        aClient = ConnectWebSocket(aForest, "A", RepositoryId).Result;

        bForest = new Forest();
        bClient = ConnectWebSocket(bForest, "B", RepositoryId).Result;

        aParent = new("parent");
        aForest.AddPartitions([new TestPartition("partition"){Data = aParent}]);
        WaitForReceived();
        bParent = ((TestPartition)bForest.Partitions.First()).Data!;
    }

    /// <inheritdoc />
    protected override string AdditionalServerParameters() =>
        TestLanguageLanguage.Instance.DataTypeTestConcept.Key;

    [Test]
    public void AddProperty()
    {
        aParent.StringValue_0_1 = "new property";
        WaitForReceived();

        AssertEquals(aParent, bParent);
    }

    [Test]
    public void ChangeProperty()
    {
        aParent.StringValue_0_1 = "new property";
        WaitForReceived();

        bParent.StringValue_0_1 = "changed property";
        WaitForReceived();

        AssertEquals(aParent, bParent);
    }

    [Test]
    public void DeleteProperty()
    {
        aParent.StringValue_0_1 = "new property";
        WaitForReceived();

        AssertEquals(aParent, bParent);

        bParent.StringValue_0_1 = null;
        WaitForReceived();

        AssertEquals(aParent, bParent);
    }
}
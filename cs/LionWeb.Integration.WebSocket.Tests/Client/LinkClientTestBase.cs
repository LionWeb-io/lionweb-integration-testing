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
// ReSharper disable InconsistentNaming

namespace LionWeb.Integration.WebSocket.Tests.Client;

public abstract class LinkClientTestBase(ServerProcesses serverProcess)
    : WebSocketClientTestBase(serverProcess, LionWebVersions.v2023_1, [TestLanguageLanguage.Instance])
{
    protected TestPartition aPartition = null!;

    protected TestPartition bPartition = null!;

    [SetUp]
    public void ConnectToServer()
    {
        aForest = new Forest();
        aClient = ConnectWebSocket(aForest, "A", RepositoryId).Result;

        bForest = new Forest();
        bClient = ConnectWebSocket(bForest, "B", RepositoryId).Result;

        aPartition = new("partition");
        aForest.AddPartitions([aPartition]);
        WaitForReceived();
        while ((bPartition = ((TestPartition?)bForest.Partitions.FirstOrDefault())!) == null)
            Thread.Sleep(100);
    }

    protected override string AdditionalServerParameters() =>
        TestLanguageLanguage.Instance.LinkTestConcept.Key;
}
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
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Protocol.Delta.Client;

namespace LionWeb.Integration.WebSocket.Tests.Client;

public abstract class LinkClientTestBase(ServerProcesses serverProcess)
    : WebSocketClientTestBase(serverProcess, LionWebVersions.v2023_1, [TestLanguageLanguage.Instance])
{
    protected LinkTestConcept aPartition;
    protected LionWebTestClient aClient;

    protected LinkTestConcept bPartition;
    protected LionWebTestClient bClient;

    [SetUp]
    public void ConnectToServer()
    {
        aPartition = new("partition");
        aClient = ConnectWebSocket(aPartition, "A").Result;

        bPartition = new("partition");
        bClient = ConnectWebSocket(bPartition, "B").Result;
    }

    protected override string AdditionalServerParameters() =>
        TestLanguageLanguage.Instance.LinkTestConcept.Key;
}
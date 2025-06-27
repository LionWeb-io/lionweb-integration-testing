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
using LionWeb.Integration.Languages.Generated.V2024_1.StructureName.M2;

namespace LionWeb.Integration.WebSocket.Tests.Client;

public abstract class SimpleClientTestBase : WebSocketClientTestBase
{
    protected const int Timeout = 6000;

    protected readonly ConceptPartition aPartition;
    protected readonly LionWebTestClient aClient;

    protected readonly ConceptPartition bPartition;
    protected readonly LionWebTestClient bClient;

    public SimpleClientTestBase() : base(LionWebVersions.v2024_1, [StructureNameLanguage.Instance])
    {
        aPartition = new("partition");
        aClient = ConnectWebSocket(aPartition, "A").Result;

        bPartition = new("partition");
        bClient = ConnectWebSocket(bPartition, "B").Result;
    }

    protected override string AdditionalServerParameters() =>
        "StructureName";
}
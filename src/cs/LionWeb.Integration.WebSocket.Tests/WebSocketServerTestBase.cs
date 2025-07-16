// Copyright 2025 LionWeb Project
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
// SPDX-FileCopyrightText: 2025 LionWeb Project
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using LionWeb.Core;
using LionWeb.Core.M3;
using LionWeb.Integration.WebSocket.Client;
using LionWeb.Integration.WebSocket.Server;

namespace LionWeb.Integration.WebSocket.Tests;

[TestFixture]
public abstract class WebSocketServerTestBase : WebSocketTestBase
{
    protected WebSocketServer _webSocketServer;

    protected WebSocketServerTestBase(LionWebVersions? lionWebVersion = null, List<Language>? languages = null) : base(lionWebVersion, languages)
    {
        Debug.WriteLine(Directory.GetCurrentDirectory());
    }

    private readonly ExternalProcessRunner _externalProcessRunner = new ();

    protected void StartClient(string name, params string[] tasks)
    {
        _externalProcessRunner.StartProcess(
            "dotnet",
            $"{Directory.GetCurrentDirectory()}/../../../../LionWeb.Integration.WebSocket.Client",
            $"""
             run
             -v q
             --property WarningLevel=0
             --property NoWarn=NU1507
             --
             {name}
             {IpAddress}
             {Port}
             {string.Join(",", tasks)}
             """,
            WebSocketClient.ClientStartedMessage
        );
    }

    [TearDown]
    public void StopClients()
    {
        _externalProcessRunner.StopAllProcesses();
        _webSocketServer.Stop();
    }
}
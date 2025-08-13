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
using LionWeb.Protocol.Delta.Client;

namespace LionWeb.Integration.WebSocket.Tests;

[TestFixture(ServerProcesses.LionWebServer)]
// [TestFixture(ServerProcesses.CSharp)]
// [TestFixture(ServerProcesses.OtherCSharp)]
public abstract class WebSocketClientTestBase : WebSocketTestBase
{
    private readonly ServerProcesses _serverProcess;
    private Process _process;

    protected WebSocketClientTestBase(ServerProcesses serverProcess, LionWebVersions? lionWebVersion = null, List<Language>? languages = null) : base(lionWebVersion, languages)
    {
        _serverProcess = serverProcess;
        Debug.WriteLine(Directory.GetCurrentDirectory());
    }

    [SetUp]
    public void StartServer()
    {
        Console.WriteLine("StartServer()");
        if (_serverProcess == ServerProcesses.LionWebServer)
        {
            var process = _serverProcess.Create(Port, AdditionalServerParameters(), out var trigger);
            _externalProcessRunner.StartProcess(
                "node",
                process.StartInfo.WorkingDirectory + "dist/",
                // cwd is assumed to be: <repo root>/src/cs/LionWeb.Integration.WebSocket.Tests/bin/Debug/net8.0
                // (hence 3x ../)
                "/server.js",
                "started"
            );
        }
        else
        {
            var process = _serverProcess.Create(Port, AdditionalServerParameters(), out var trigger);
            _externalProcessRunner.StartProcess(process, trigger);
        }
    }

    protected virtual string AdditionalServerParameters() =>
        "";

    protected async Task<LionWebTestClient> ConnectWebSocket(IPartitionInstance partition, string name)
    {
        var webSocket = new WebSocketClient(name);
        var lionWeb = new LionWebTestClient(_lionWebVersion, _languages, $"client_{name}", partition, webSocket);
        await webSocket.ConnectToServer(IpAddress, Port);
        await lionWeb.SignOn();
        return lionWeb;
    }
}
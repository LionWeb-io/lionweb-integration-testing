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
using LionWeb.Core.M1;
using LionWeb.Core.M3;
using LionWeb.Protocol.Delta.Client;
using LionWeb.WebSocket;

namespace LionWeb.Integration.WebSocket.Tests;

// [TestFixture(ServerProcesses.LionWebServer)]
[TestFixture(ServerProcesses.CSharp)]
public abstract class WebSocketClientTestBase : WebSocketTestBase
{
    private readonly ServerProcesses _serverProcess;

    protected IForest aForest = null!;
    protected LionWebTestClient aClient = null!;
    protected IForest bForest = null!;
    protected LionWebTestClient bClient = null!;

    protected WebSocketClientTestBase(ServerProcesses serverProcess, LionWebVersions? lionWebVersion = null,
        List<Language>? languages = null) : base(lionWebVersion, languages)
    {
        _serverProcess = serverProcess;
        Debug.WriteLine(Directory.GetCurrentDirectory());
    }

    [SetUp]
    public void StartServer()
    {
        Console.WriteLine("StartServer()");
        var process = _serverProcess.Create(Port, AdditionalServerParameters(), out var readyTrigger,
            out var errorTrigger);
        _externalProcessRunner.StartProcess(process, readyTrigger, errorTrigger);
    }

    protected virtual string AdditionalServerParameters() =>
        "";

    protected async Task<LionWebTestClient> ConnectWebSocket(IForest forest, string name, RepositoryId repositoryId)
    {
        var webSocket = new WebSocketClient(name, _lionWebVersion);
        var lionWeb = new LionWebTestClient(_lionWebVersion, _languages, $"client_{name}", forest, webSocket.Connector);

        await webSocket.ConnectToServer(IpAddress, Port);
        await lionWeb.SignOn(repositoryId);
        await lionWeb.SubscribeToChangingPartitions(creation: true, deletion: true, partitions: true);

        lionWeb.WaitForReceived(2);
        return lionWeb;
    }

    protected void WaitForReceived(int numberOfMessages = 1)
    {
        long aMessageCount = aClient.WaitCount += numberOfMessages;
        long bMessageCount = bClient.WaitCount += numberOfMessages;

        while (!_externalProcessRunner.ErrorTriggerEncountered
               && (
                   aClient.MessageCount < aMessageCount
                   || bClient.MessageCount < bMessageCount
               )
              )
        {
            Thread.Sleep(LionWebTestClient._sleepInterval);
        }

        if (_externalProcessRunner.ErrorTriggerEncountered)
            Assert.Fail("repo failure");
    }
}
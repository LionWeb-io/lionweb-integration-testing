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
using LionWeb.Integration.WebSocket.Client;
using LionWeb.Integration.WebSocket.Server;
using LionWeb.Protocol.Delta.Client;
using LionWeb.Protocol.Delta.Repository;
using NUnit.Framework.Legacy;

namespace LionWeb.Integration.WebSocket.Tests;

public abstract class WebSocketServerTestBase : WebSocketTestBase
{
    private readonly ClientProcesses[] _clientProcesses;
    private int nextClientProcess;
    protected WebSocketServer _webSocketServer;
    protected LionWebTestRepository lionWebServer;

    protected WebSocketServerTestBase(params ClientProcesses[] clientProcesses) : base(null, null)
    {
        _clientProcesses = clientProcesses;
        Debug.WriteLine(Directory.GetCurrentDirectory());
    }

    [SetUp]
    public void ResetClientProcessCount()
    {
        nextClientProcess = 0;
    }

    private Process NextProcess(string name, Type partitionType, Tasks[] tasks, out string readyTrigger,
        out string errorTrigger) =>
        _clientProcesses[nextClientProcess++ % _clientProcesses.Length]
            .Create(name, partitionType.Name, Port, tasks.Select(t => Enum.GetName(t)), out readyTrigger, out errorTrigger);

    protected void StartClient(string name, Type partitionType, params Tasks[] tasks)
    {
        var process = NextProcess(name, partitionType, tasks, out var readyTrigger, out var errorTrigger);
        _externalProcessRunner.StartProcess(process, readyTrigger, errorTrigger);

        ClassicAssert.IsFalse(process.HasExited);
    }

    [TearDown]
    [OneTimeTearDown]
    public void StopServer()
    {
        _webSocketServer.Stop();
    }

    protected void WaitForSent(int delta = 1)
    {
        long count = lionWebServer.WaitSentCount += delta;
        while (!_externalProcessRunner.ShouldCancel && lionWebServer.MessageSentCount < count)
        {
            Thread.Sleep(LionWebTestClient._sleepInterval);
        }

        if (_externalProcessRunner.ShouldCancel)
            Assert.Fail("repo failure");
    }
}
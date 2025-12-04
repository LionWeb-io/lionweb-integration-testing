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
using LionWeb.Protocol.Delta.Client;
using LionWeb.Protocol.Delta.Repository;
using LionWeb.WebSocket;
using NUnit.Framework.Legacy;
// ReSharper disable InconsistentNaming

namespace LionWeb.Integration.WebSocket.Tests;

[TestFixture(ClientProcesses.CSharp)]
[TestFixture(ClientProcesses.Ts)]
public abstract class WebSocketServerTestBase : WebSocketTestBase
{
    private readonly ClientProcesses[] _clientProcesses;
    private int _nextClientProcess;
    protected WebSocketServer _webSocketServer = null!;
    protected LionWebTestRepository lionWebServer = null!;

    protected WebSocketServerTestBase(params ClientProcesses[] clientProcesses) : base(null, null)
    {
        _clientProcesses = clientProcesses;
        Debug.WriteLine(Directory.GetCurrentDirectory());
    }

    [SetUp]
    public void ResetClientProcessCount()
    {
        _nextClientProcess = 0;
    }

    private Process NextProcess(string name, Type partitionType, Tasks[] tasks, out string readyTrigger,
        out string errorTrigger) =>
        _clientProcesses[_nextClientProcess++ % _clientProcesses.Length]
            .Create(name, partitionType.Name, Port, tasks.Select(Enum.GetName)!, out readyTrigger, out errorTrigger);

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
    }

    protected void WaitForSent(int numberOfMessages = 1)
    {
        long messageCount = lionWebServer.WaitSentCount += numberOfMessages;
        Console.WriteLine($"WaitSentCount: {lionWebServer.WaitSentCount} MessageSentCount {lionWebServer.MessageSentCount}");
        while (!_externalProcessRunner.ErrorTriggerEncountered && lionWebServer.MessageSentCount < messageCount)
        {
            Console.WriteLine($"messageCount: {messageCount} MessageSentCount {lionWebServer.MessageSentCount}");
            Thread.Sleep(LionWebTestClient._sleepInterval);
        }

        if (_externalProcessRunner.ErrorTriggerEncountered)
            Assert.Fail("client failure");
    }
}
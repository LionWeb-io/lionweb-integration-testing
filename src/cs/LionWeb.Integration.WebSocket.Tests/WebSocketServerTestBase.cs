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
using NUnit.Framework.Legacy;

namespace LionWeb.Integration.WebSocket.Tests;

[TestFixture]
public abstract class WebSocketServerTestBase : WebSocketTestBase
{
    private readonly List<Process> _processes = [];

    protected WebSocketServer _webSocketServer;

    protected WebSocketServerTestBase(LionWebVersions? lionWebVersion = null, List<Language>? languages = null) : base(lionWebVersion, languages)
    {
        Debug.WriteLine(Directory.GetCurrentDirectory());
    }

    protected void StartClient(string name, params string[] tasks)
    {
        var process = new Process();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../LionWeb.Integration.WebSocket.Client";
        process.StartInfo.Arguments = $"""
                                       run
                                       -v q
                                       --property WarningLevel=0
                                       --property NoWarn=NU1507
                                       --
                                       {name}
                                       {IpAddress}
                                       {Port}
                                       {string.Join(",", tasks)}
                                       """.ReplaceLineEndings(" ");
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;

        var clientStarted = false;

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data?.Contains(WebSocketClient.ClientStartedMessage) ?? false)
                clientStarted = true;
            Console.WriteLine(args.Data);
        };
        process.ErrorDataReceived += (sender, args) => Console.Error.WriteLine(args.Data);

        Assert.That(process.Start());
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        while (!clientStarted)
        {
            Thread.Sleep(100);
        }

        ClassicAssert.IsFalse(process.HasExited);

        _processes.Add(process);
    }

    [TearDown]
    public void StopClients()
    {
        foreach (var process in _processes)
        {
            if (process.HasExited)
                continue;

            TestContext.WriteLine($"Killing WebSocket Client {process.ProcessName}");
            process.Kill();
        }

        if (_webSocketServer != null)
        {
            _webSocketServer.Stop();
        }
    }
}
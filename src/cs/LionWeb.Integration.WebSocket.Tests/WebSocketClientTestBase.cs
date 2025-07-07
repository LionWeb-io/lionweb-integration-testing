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
using LionWeb.Core.Utilities;
using LionWeb.Integration.WebSocket.Client;
using LionWeb.Integration.WebSocket.Server;
using LionWeb.Protocol.Delta.Client;
using LionWeb.Protocol.Delta.Message.Query;

namespace LionWeb.Integration.WebSocket.Tests;

[TestClass]
public abstract class WebSocketClientTestBase : WebSocketTestBase, IDisposable
{
    private Process _process;

    protected WebSocketClientTestBase(LionWebVersions? lionWebVersion = null, List<Language>? languages = null) : base(lionWebVersion, languages)
    {
        Debug.WriteLine(Directory.GetCurrentDirectory());
        StartServer();
    }

    private void StartServer()
    {
        _process = new Process();
        _process.StartInfo.FileName = "dotnet";
        _process.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../LionWeb.Integration.WebSocket.Server";
        _process.StartInfo.Arguments = $"""
                                        run
                                        -v q
                                        --property WarningLevel=0
                                        --property NoWarn=NU1507
                                        {AdditionalServerParameters()}
                                        """.ReplaceLineEndings(" ");
        _process.StartInfo.UseShellExecute = false;
        _process.StartInfo.RedirectStandardError = true;
        _process.StartInfo.RedirectStandardInput = true;
        _process.StartInfo.RedirectStandardOutput = true;

        var serverStarted = false;

        _process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data?.Contains(WebSocketServer.ServerStartedMessage) ?? false)
                serverStarted = true;
            Console.WriteLine(args.Data);
        };
        _process.ErrorDataReceived += (sender, args) => Console.Error.WriteLine(args.Data);

        Assert.IsTrue(_process.Start());
        _process.BeginErrorReadLine();
        _process.BeginOutputReadLine();

        while (!serverStarted)
        {
            Thread.Sleep(100);
        }

        Assert.IsFalse(_process.HasExited);
    }

    protected virtual string AdditionalServerParameters() =>
        "";

    /// <inheritdoc />
    [TestCleanup()]
    [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass, ClassCleanupBehavior.EndOfClass)]
    public void Dispose()
    {
        if (_process == null || _process.HasExited)
            return;

        try
        {
            TestContext.WriteLine($"Killing WebSocket Server {_process.ProcessName}");
            _process.Kill();
        }
        catch (Exception e)
        {
            TestContext.WriteLine("Dispose:" + e);
        }
    }

    protected async Task<LionWebTestClient> ConnectWebSocket(IPartitionInstance partition, string name)
    {
        var webSocket = new WebSocketClient(name);
        var lionWeb = new LionWebTestClient(_lionWebVersion, _languages, $"client_{name}", partition, webSocket);
        await webSocket.ConnectToServer(IpAddress, Port);
        await lionWeb.SignOn();
        return lionWeb;
    }
}
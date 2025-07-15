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
using LionWeb.Protocol.Delta.Client;
using NUnit.Framework.Legacy;

namespace LionWeb.Integration.WebSocket.Tests;

public enum ServerProcesses
{
    CSharp,
    OtherCSharp
}

[TestFixture(ServerProcesses.CSharp)]
// [TestFixture(ServerProcesses.OtherCSharp)]
public abstract class WebSocketClientTestBase : WebSocketTestBase
{
    private readonly ServerProcesses _serverProcess;
    private Process _process;

    private Process CSharpServer()
    {
        TestContext.WriteLine($"AdditionalServerParameters: {AdditionalServerParameters()}");
        var result = new Process();
        result.StartInfo.FileName = "dotnet";
        result.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../LionWeb.Integration.WebSocket.Server";
        result.StartInfo.Arguments = $"""
                                        run
                                        -v q
                                        --property WarningLevel=0
                                        --property NoWarn=NU1507
                                        {AdditionalServerParameters()}
                                        """.ReplaceLineEndings(" ");
        result.StartInfo.UseShellExecute = false;
        return result;
    }

    protected WebSocketClientTestBase(ServerProcesses serverProcess, LionWebVersions? lionWebVersion = null, List<Language>? languages = null) : base(lionWebVersion, languages)
    {
        _serverProcess = serverProcess;
        Debug.WriteLine(Directory.GetCurrentDirectory());
    }

    [SetUp]
    public void StartServer()
    {
        Console.WriteLine("StartServer()");
        // cleans out leftovers
        StopServer();
        
        _process = _serverProcess switch
        {
            ServerProcesses.CSharp => CSharpServer(),
            ServerProcesses.OtherCSharp => CSharpServer(),
            _ => throw new ArgumentOutOfRangeException(nameof(_serverProcess), _serverProcess, null)
        };
        
        _allServers.Add(_process);
        
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

        Assert.That(_process.Start());
        _process.BeginErrorReadLine();
        _process.BeginOutputReadLine();

        while (!serverStarted)
        {
            Thread.Sleep(100);
        }

        ClassicAssert.IsFalse(_process.HasExited);
    }

    protected virtual string AdditionalServerParameters() =>
        "";

    private static readonly List<Process> _allServers = []; 
    
    [TearDown]
    [OneTimeTearDown]
    public void StopServer()
    {
        // We loop over _allServers, as [TearDown] will not be executed after a [Timeout].
        // Then, we at least kill all previous servers in the next run.
        foreach (var server in _allServers)
        {
            if (server.HasExited) continue;
            try
            {
                TestContext.WriteLine($"Killing WebSocket Server {server.ProcessName}");
                server.Kill();
            }
            catch (Exception e)
            {
                TestContext.WriteLine("StopServer:" + e);
            }
        }
        
        _allServers.Clear();
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
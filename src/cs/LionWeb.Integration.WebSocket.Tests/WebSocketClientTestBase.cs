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
using LionWeb.Integration.Languages;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using LionWeb.Integration.WebSocket.Client;
using LionWeb.Protocol.Delta.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LionWeb.Integration.WebSocket.Tests;

[TestClass]
public abstract class WebSocketClientTestBase : IDisposable
{
    private readonly LionWebVersions _lionWebVersion;

    private readonly List<Language> _languages;

    private const string IpAddress = "localhost";
    private const int Port = 42424;

    private Process _process;

    protected const int TestTimeout = 6000;
    private const int ServerStartTimeout = 500;

    protected WebSocketClientTestBase(LionWebVersions? lionWebVersion = null, List<Language>? languages = null)
    {
        _lionWebVersion = lionWebVersion ?? LionWebVersions.v2023_1;
        _languages = languages ?? [ShapesLanguage.Instance];
        _languages.AddRange([_lionWebVersion.BuiltIns, _lionWebVersion.LionCore]);

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

        _process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        _process.ErrorDataReceived += (sender, args) => Console.Error.WriteLine(args.Data);

        Assert.IsTrue(_process.Start());
        _process.BeginErrorReadLine();
        _process.BeginOutputReadLine();
        Thread.Sleep(ServerStartTimeout);
        Assert.IsFalse(_process.HasExited);
    }

    protected virtual string AdditionalServerParameters() =>
        "";

    public void Dispose()
    {
        TestContext.WriteLine($"Killing WebSocket Server {_process.ProcessName}");
        _process?.Kill();
    }

    public TestContext TestContext { get; set; }

    protected void AssertEquals(INode? a, INode? b) =>
        AssertEquals([a], [b]);

    protected void AssertEquals(IEnumerable<INode?> a, IEnumerable<INode?> b)
    {
        List<IDifference> differences = new Comparer(a.ToList(), b.ToList()).Compare().ToList();
        Assert.IsTrue(differences.Count == 0,
            differences.DescribeAll(new() { LeftDescription = "a", RightDescription = "b" }));
    }

    protected async Task<LionWebTestClient> ConnectWebSocket(IPartitionInstance partition, string name)
    {
        var webSocket = new WebSocketClient(name);
        var lionWeb = new LionWebTestClient(_lionWebVersion, _languages, $"client_{name}", partition, webSocket);
        await webSocket.ConnectToServer(IpAddress, Port);
        await lionWeb.Send(new SignOnRequest("2025.1", IdUtils.NewId(), null));
        lionWeb.WaitForReplies(1);
        return lionWeb;
    }
}
// // Copyright 2024 TRUMPF Laser GmbH
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// // SPDX-FileCopyrightText: 2024 TRUMPF Laser GmbH
// // SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using LionWeb.Core;
using LionWeb.Core.M3;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LionWeb.Integration.WebSocket.Tests;

[TestClass]
public abstract class WebSocketClientTestBase : IDisposable
{
    private Process _process;

    protected LionWebVersions LionWebVersion { get; init; } = LionWebVersions.v2023_1;
    public List<Language> Languages { get; init; } = [ShapesLanguage.Instance];
    

    protected WebSocketClientTestBase()
    {
        Console.WriteLine(Directory.GetCurrentDirectory());
        StartServer();
    }

    private void StartServer()
    {
        _process = new Process();
        _process.StartInfo.FileName = "dotnet";
        _process.StartInfo.WorkingDirectory = $"{Directory.GetCurrentDirectory()}/../../../../LionWeb.Integration.WebSocket.Server";
        _process.StartInfo.Arguments = """
                                       run
                                       -v q
                                       --nologo
                                       --property WarningLevel=0
                                       --property NoWarn=NU1507
                                       /clp:ErrorsOnly
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
        Thread.Sleep(500);
        Assert.IsFalse(_process.HasExited);
    }

    public void Dispose()
    {
        TestContext.WriteLine($"Killing WebSocket Server {_process.ProcessName}");
        _process?.Kill();
    }

    public TestContext TestContext { get; set; }
}
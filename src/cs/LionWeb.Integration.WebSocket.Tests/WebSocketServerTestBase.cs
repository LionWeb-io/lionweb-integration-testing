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
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LionWeb.Integration.WebSocket.Tests;

[TestClass]
public abstract class WebSocketServerTestBase : IDisposable
{
    private readonly List<Process> _processes= [];

    protected const string IpAddress = "localhost";
    protected const int Port = 42424;
    
    protected LionWebVersions LionWebVersion { get; init; } = LionWebVersions.v2023_1;
    protected List<Language> Languages { get; init; } = [ShapesLanguage.Instance];
    

    protected WebSocketServerTestBase()
    {
        Debug.WriteLine(Directory.GetCurrentDirectory());
    }

    protected void StartClient(string name, params string[] tasks)
    {
        var process = new Process();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.WorkingDirectory = $"{Directory.GetCurrentDirectory()}/../../../../LionWeb.Integration.WebSocket.Client";
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
        
        process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        process.ErrorDataReceived += (sender, args) => Console.Error.WriteLine(args.Data);
        
        Assert.IsTrue(process.Start());
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        Thread.Sleep(500);
        Assert.IsFalse(process.HasExited);
        
        _processes.Add(process);
    }

    public void Dispose()
    {
        foreach (var process in _processes)
        {
            if(process.HasExited)
                continue;
            
            TestContext.WriteLine($"Killing WebSocket Client {process.ProcessName}");
            process.Kill();
        }
    }

    public TestContext TestContext { get; set; }
    
    protected void AssertEquals(INode? expected, INode? actual) =>
        AssertEquals([expected], [actual]);
    
    protected void AssertEquals(IEnumerable<INode?> expected, IEnumerable<INode?> actual)
    {
        List<IDifference> differences = new Comparer(expected.ToList(), actual.ToList()).Compare().ToList();
        Assert.IsTrue(differences.Count == 0, differences.DescribeAll(new()));
    }
}
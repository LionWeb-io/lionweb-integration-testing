// Copyright 2024 TRUMPF Laser GmbH
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
// SPDX-FileCopyrightText: 2024 TRUMPF Laser GmbH
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Runtime.InteropServices;
using LionWeb.Integration.WebSocket.Client;

namespace LionWeb.Integration.WebSocket.Tests;

public enum ClientProcesses
{
    CSharp,
    Ts
}

public static class ClientProcessesExtensions
{
    public static Process Create(this ClientProcesses process, string name, string partitionType, int port,
        IEnumerable<string> tasks, out string readyTrigger, out string errorTrigger) => process switch
    {
        ClientProcesses.CSharp => CSharpClient(name, partitionType, port, tasks, out readyTrigger, out errorTrigger),
        ClientProcesses.Ts => TsClient(name, partitionType, port, tasks, out readyTrigger, out errorTrigger),
        _ => throw new ArgumentOutOfRangeException(nameof(process), process, null)
    };

    private static Process CSharpClient(string name, string partitionType, int port, IEnumerable<string> tasks,
        out string readyTrigger, out string errorTrigger)
    {
        var result = new Process();
        result.StartInfo.FileName = "dotnet";
        result.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../LionWeb.Integration.WebSocket.Client";
        result.StartInfo.Arguments = $"""
                                      run
                                      --no-build
                                      --
                                      {name}
                                      {WebSocketTestBase.IpAddress}
                                      {port}
                                      {partitionType}
                                      {string.Join(",", tasks)}
                                      """.ReplaceLineEndings(" ");
        result.StartInfo.UseShellExecute = false;
        readyTrigger = WebSocketClient.ClientStartedMessage;
        errorTrigger = "Error";
        return result;
    }

    private static Process TsClient(string clientId, string partitionType, int port, IEnumerable<string> tasks,
        out string trigger,
        out string errorTrigger)
    {
        string fileName = "npx";
        string argumentPrefix = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileName = "powershell";
            argumentPrefix = "npx ";
        }
        
        var result = new Process();
        result.StartInfo.FileName = fileName;
        // Note: the following line means that it's assumed that
        //  1) the lionweb-typescript repo is located right next to the lionweb-integration-testing repo,
        //  2) that the latter repo has been checked out on the delta-protocol-impl branch,
        //  3) and that it's been built entirely successfully.
        // result.StartInfo.WorkingDirectory =
        //     $"{Directory.GetCurrentDirectory()}/../../../../../../../lionweb-typescript/packages/deltas-websocket";
        // cwd is assumed to be: <LionWeb dir.>/lionweb-integration-testing/src/cs/LionWeb.Integration.WebSocket.Tests/bin/Debug/net8.0
        // (hence 7x ../)
        // result.StartInfo.Arguments = $"dist/cli/client.js {port} {clientId} {string.Join(",", tasks)}";
        result.StartInfo.Arguments = $"{argumentPrefix}--package=@lionweb/delta-protocol-test-cli@0.7.0-beta.13 cli-client {port} {clientId} {partitionType} {string.Join(",", tasks)}";
        result.StartInfo.UseShellExecute = false;
        trigger = "LionWeb delta protocol client";
        errorTrigger = "Error";
        return result;
    }
}
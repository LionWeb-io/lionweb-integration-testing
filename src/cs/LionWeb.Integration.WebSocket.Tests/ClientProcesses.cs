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

    /// <remarks>
    /// This method assumes that
    /// <list type="number">
    /// <item>environment variable <see cref="TsDeltaCliVersionEnvironmentVariable"/> is set.</item>
    /// <item><c>npx --package=@lionweb/delta-protocol-test-cli@{TsDeltaCliVersion}</c> executes successfully and within the timeout.
    ///     Execute it once before running tests to make sure everything is already downloaded and cached locally.</item>
    /// </list>
    /// </remarks>
    private static Process TsClient(string clientId, string partitionType, int port, IEnumerable<string> tasks,
        out string trigger,
        out string errorTrigger)
    {
        // On Windows, we need to call `powershell npx` instead of `npx`.
        string fileName = "npx";
        string argumentPrefix = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileName = "powershell";
            argumentPrefix = "npx ";
        }

        var result = new Process();
        result.StartInfo.FileName = fileName;
        result.StartInfo.Arguments =
            $"{argumentPrefix}--package=@lionweb/delta-protocol-test-cli@{TsDeltaCliVersion} cli-client {port} {clientId} {partitionType} {string.Join(",", tasks)}";
        result.StartInfo.UseShellExecute = false;
        trigger = "LionWeb delta protocol client";
        errorTrigger = "Error";
        return result;
    }

    private const string TsDeltaCliVersionEnvironmentVariable = "TS_DELTA_CLI_VERSION";

    /// <remarks>
    /// There's a
    /// <a href="https://www.jetbrains.com/help/rider/Reference__Options__Tools__Unit_Testing__Test_Runner.html#environment-variables">special place</a>
    /// for setting test environment variables in Rider. 
    /// </remarks>
    private static string TsDeltaCliVersion =>
        Environment.GetEnvironmentVariable(TsDeltaCliVersionEnvironmentVariable) ??
        throw new ArgumentException($"environment variable {TsDeltaCliVersionEnvironmentVariable} not set");
}
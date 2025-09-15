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

    #region CSharpClient

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
                                      --configuration {Configuration}
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
    
    internal static string Configuration => AssemblyConfigurationAttribute.Get("Configuration");

    #endregion

    #region TypeScript client

    private static Process TsClient(string clientId, string partitionType, int port, IEnumerable<string> tasks,
        out string trigger,
        out string errorTrigger)
    {
        var cmdLine = $"{port} {clientId} {partitionType} {string.Join(",", tasks)}";

// Accesses `<DefineConstants>USE_LION_WEB_PACKAGES</DefineConstants>` from .csproj 
#if USE_LION_WEB_PACKAGES
        var result = TsNpxPackageClient(cmdLine);
#else
        var result = TsRelativeDirectoryClient(cmdLine);
#endif

        result.StartInfo.UseShellExecute = false;
        trigger = "LionWeb delta protocol client";
        errorTrigger = "Error";
        return result;
    }

    /// <remarks>
    /// This method assumes that
    /// <list type="number">
    ///  <item>the lionweb-typescript repo is located right next to the lionweb-integration-testing repo,</item>
    ///  <item>that the latter repo has been checked out on the delta-protocol-impl branch,</item>
    ///  <item>and that it's been built entirely successfully.</item>
    /// </list>
    /// </remarks>
    private static Process TsRelativeDirectoryClient(string cmdLine)
    {
        var result = new Process();
        result.StartInfo.FileName = "node";
        result.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../../../../lionweb-typescript/packages/deltas-websocket";
        // cwd is assumed to be: <LionWeb dir.>/lionweb-integration-testing/src/cs/LionWeb.Integration.WebSocket.Tests/bin/Debug/net8.0
        // (hence 7x ../)
        result.StartInfo.Arguments = $"dist/cli/client.js {cmdLine}";

        return result;
    }

    /// <remarks>
    /// This method assumes that
    /// <list type="number">
    /// <item><i>Directory.Packages.props</i> property <c>LionWebTsVersion</c> is set.</item>
    /// <item><c>npx --package=@lionweb/delta-protocol-test-cli@{TsDeltaCliVersion}</c> executes successfully and within the timeout.
    ///     Execute it once before running tests to make sure everything is already downloaded and cached locally.</item>
    /// </list>
    /// </remarks>
    private static Process TsNpxPackageClient(string cmdLine)
    {
        var result = new Process();
        var npxArg = $"--package=@lionweb/delta-protocol-test-cli@{TsDeltaCliVersion} cli-client";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // On Windows, we need to call `powershell npx` instead of `npx`.
            result.StartInfo.FileName = "powershell";
            result.StartInfo.Arguments = $"npx {npxArg} {cmdLine}";
        }
        else
        {
            result.StartInfo.FileName = "npx";
            result.StartInfo.Arguments = $"${npxArg} ${cmdLine}";
        }

        return result;
    }

    internal static string TsDeltaCliVersion => AssemblyConfigurationAttribute.Get("LionWebTsVersion");

    #endregion
}
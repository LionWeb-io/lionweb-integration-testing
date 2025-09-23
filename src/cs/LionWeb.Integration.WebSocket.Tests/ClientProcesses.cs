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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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
        var process = new Process();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../LionWeb.Integration.WebSocket.Client";
        process.StartInfo.Arguments = $"""
                                      run
                                      --no-build
                                      --configuration {AssemblyConfigurationAttribute.Configuration}
                                      --
                                      {name}
                                      {WebSocketTestBase.IpAddress}
                                      {port}
                                      {partitionType}
                                      {string.Join(",", tasks)}
                                      """.ReplaceLineEndings(" ");
        process.StartInfo.UseShellExecute = false;
        readyTrigger = WebSocketClient.ClientStartedMessage;
        errorTrigger = "Exception";
        
        return process;
    }

    #endregion

    #region TypeScript client

    private static Process TsClient(string clientId, string partitionType, int port, IEnumerable<string> tasks,
        out string trigger,
        out string errorTrigger)
    {
        var cmdLine = $"{port} {clientId} {partitionType} {string.Join(",", tasks)}";

// Accesses `<DefineConstants>USE_LION_WEB_PACKAGES</DefineConstants>` from .csproj 
#if USE_LION_WEB_PACKAGES
        var process = TsNpxClientPackage(cmdLine);
#else
        var process = TsRelativeDirectoryClient(cmdLine);
#endif

        process.StartInfo.UseShellExecute = false;
        trigger = "LionWeb delta protocol client";
        errorTrigger = "Error";
        return process;
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
        var process = new Process();
        process.StartInfo.FileName = "node";
        process.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../../../../lionweb-typescript/packages/delta-protocol-test-cli";
        // cwd is assumed to be: <LionWeb dir.>/lionweb-integration-testing/src/cs/LionWeb.Integration.WebSocket.Tests/bin/Debug/net8.0
        // (hence 7x ../)
        process.StartInfo.Arguments = $"dist/cli-client.js {cmdLine}";

        return process;
    }

    private static Process CreateNodeUtilityProcess(params string[] arguments)
    {
        var process = new Process();
        var effectiveArguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ["powershell", ..arguments]   // on Windows, we need to call `powershell npx` instead of `npx`
            : arguments;
        process.StartInfo.FileName = effectiveArguments[0];
        process.StartInfo.Arguments = string.Join(" ", effectiveArguments[1..]);
        return process;
    }

    /// <remarks>
    /// This method assumes that
    /// <list type="number">
    /// <item><i>Directory.Packages.props</i> property <c>LionWebTsVersion</c> is set.</item>
    /// <item>the <c>@lionweb/delta-protocol-test-cli</c> NPM package has been downloaded and cached locally.</item>
    /// </list>
    /// </remarks>
    private static Process TsNpxClientPackage(string cmdLine)
        => CreateNodeUtilityProcess("npx", "cli-client", cmdLine);

    internal static string TsDeltaCliVersion => AssemblyConfigurationAttribute.Get("LionWebTsVersion");

    internal static Process TsInstallClientPackage()
        => CreateNodeUtilityProcess("npm", "install", $"@lionweb/delta-protocol-test-cli@{TsDeltaCliVersion}");

    internal static Process? SetUpTsClient()
    {
// Accesses `<DefineConstants>USE_LION_WEB_PACKAGES</DefineConstants>` from .csproj 
#if USE_LION_WEB_PACKAGES
        return TsInstallClientPackage();
#else
        return null;
#endif

    }
    
    #endregion
}
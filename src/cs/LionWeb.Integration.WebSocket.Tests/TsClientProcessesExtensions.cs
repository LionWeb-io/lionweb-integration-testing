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

namespace LionWeb.Integration.WebSocket.Tests;

public static class TsClientProcessesExtensions
{
    public static Process TsClient(string clientId, string partitionType, int port, IEnumerable<string> tasks,
        out string readyTrigger,
        out string errorTrigger)
    {
        var cmdLine = $"{port} {clientId} {partitionType} {string.Join(",", tasks)}";

// Accesses `<DefineConstants>USE_LION_WEB_PACKAGES</DefineConstants>` from .csproj 
#if USE_LION_WEB_PACKAGES
        var process = TsNpxClient(cmdLine);
#else
        var process = TsRelativeDirectoryClient(cmdLine);
#endif

        process.StartInfo.UseShellExecute = false;
        readyTrigger = "LionWeb delta protocol client";
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
    // ReSharper disable once UnusedMember.Local
    private static Process TsNpxClient(string cmdLine)
        => CreateNodeUtilityProcess("npx", "cli-client", cmdLine);

    internal static string LionWebTsVersion => AssemblyConfigurationAttribute.Get("LionWebTsVersion");

    internal static Process TsInstallClientPackage()
        => CreateNodeUtilityProcess("npm", "install", $"@lionweb/delta-protocol-test-cli@{LionWebTsVersion}");

    internal static Process? SetUpTsClient()
    {
// Accesses `<DefineConstants>USE_LION_WEB_PACKAGES</DefineConstants>` from .csproj 
#if USE_LION_WEB_PACKAGES
        return TsInstallClientPackage();
#else
        return null;
#endif
    }
}
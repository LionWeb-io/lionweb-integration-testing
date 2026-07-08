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
    public static Process TsClient(string clientId, int port, IEnumerable<string> tasks,
        out string readyTrigger,
        out string errorTrigger)
    {
        var cmdLine = $"{port} {clientId} {string.Join(",", tasks)}";
        var process = TsRelativeDirectoryClient(cmdLine);

        process.StartInfo.UseShellExecute = false;
        readyTrigger = "LionWeb delta protocol client";
        errorTrigger = "Error";
        return process;
    }

    private static Process TsRelativeDirectoryClient(string cmdLine)
    {
        var process = new Process();
        process.StartInfo.FileName = "node";
        process.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../../ts";
        // cwd is assumed to be: <LionWeb dir.>/lionweb-integration-testing/cs/LionWeb.Integration.WebSocket.Tests/bin/Debug/net8.0
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

    internal static string LionWebTsVersion => AssemblyConfigurationAttribute.Get("LionWebTsVersion");

    internal static Process TsInstallClientPackage()
        => CreateNodeUtilityProcess("npm", "install", $"@lionweb/delta-protocol-test-cli@{LionWebTsVersion}");

    internal static Process? SetUpTsClient()
    {
        return TsInstallClientPackage();
    }
}
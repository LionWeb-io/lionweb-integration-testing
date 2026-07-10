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
        var process = LocalProcess("node", "ts/dist/cli-client.js", cmdLine);

        process.StartInfo.UseShellExecute = false;
        readyTrigger = "LionWeb delta protocol client";
        errorTrigger = "Error";
        return process;
    }

    private static Process LocalProcess(string executable, params string[] arguments)
    {
        var process = new Process();
        process.StartInfo.FileName = executable;
        process.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../..";
        // cwd is assumed to be: <LionWeb dir.>/lionweb-integration-testing/cs/LionWeb.Integration.WebSocket.Tests/bin/Debug/net8.0
        // (hence 5x ../)
        process.StartInfo.Arguments = string.Join(" ", arguments);
        return process;
    }

    internal static string LionWebTsVersion => AssemblyConfigurationAttribute.Get("LionWebTsVersion");

    internal static Process? BuildTsClient()
        => LocalProcess(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "powershell" : "pwsh",
            "scripts/build-ts-client.ps1",
            LionWebTsVersion
        );
}
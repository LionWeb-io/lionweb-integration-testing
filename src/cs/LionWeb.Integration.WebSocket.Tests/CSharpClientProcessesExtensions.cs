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
using LionWeb.WebSocket;

namespace LionWeb.Integration.WebSocket.Tests;

public static class CSharpClientProcessesExtensions
{
    public static Process CSharpClient(string name, int port, IEnumerable<string> tasks,
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
                                       {string.Join(",", tasks)}
                                       """.ReplaceLineEndings(" ");
        process.StartInfo.UseShellExecute = false;
        readyTrigger = WebSocketClient.ClientStartedMessage;
        errorTrigger = "Exception";

        return process;
    }
}
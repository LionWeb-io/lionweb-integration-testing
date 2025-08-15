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
using LionWeb.Integration.WebSocket.Server;

namespace LionWeb.Integration.WebSocket.Tests;

public enum ServerProcesses
{
    CSharp,
    OtherCSharp
}

public static class ServerProcessesExtensions
{
    public static Process Create(this ServerProcesses process, int port, string additionalServerParameters,
        out string readyTrigger, out string errorTrigger) => process switch
    {
        ServerProcesses.CSharp => CSharpServer(port, additionalServerParameters, out readyTrigger, out errorTrigger),
        ServerProcesses.OtherCSharp => CSharpServer(port, additionalServerParameters, out readyTrigger,
            out errorTrigger),
        _ => throw new ArgumentOutOfRangeException(nameof(process), process, null)
    };

    private static Process CSharpServer(int port, string additionalServerParameters, out string readyTrigger,
        out string errorTrigger)
    {
        TestContext.WriteLine($"AdditionalServerParameters: {additionalServerParameters}");
        var result = new Process();
        result.StartInfo.FileName = "dotnet";
        result.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../LionWeb.Integration.WebSocket.Server";
        result.StartInfo.Arguments = $"""
                                      run
                                      --no-build
                                      {port}
                                      {additionalServerParameters}
                                      """.ReplaceLineEndings(" ");
        result.StartInfo.UseShellExecute = false;
        readyTrigger = WebSocketServer.ServerStartedMessage;
        errorTrigger = "Error";
        return result;
    }
}
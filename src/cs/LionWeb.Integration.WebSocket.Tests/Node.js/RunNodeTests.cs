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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace LionWeb.Integration.WebSocket.Tests.Node.js;

public class RunNodeTests
{
    private readonly ExternalProcessRunner _externalProcessRunner = new();

    [Test]
    public void RunNodeProgram()
    {
        _externalProcessRunner.StartProcess(
            "node",
            $"{Directory.GetCurrentDirectory()}/../../../Node.js",
            // cwd is assumed to be: <repo root>/src/cs/LionWeb.Integration.WebSocket.Tests/bin/Debug/net8.0
            // (hence 3x ../)
            "node-program.js",
            "started",
            "Error"
        );
    }

    [Test]
    public void RunNodeProgram_With_Error()
    {
        _externalProcessRunner.StartProcess(
            "node",
            $"{Directory.GetCurrentDirectory()}/../../../Node.js",
            // cwd is assumed to be: <repo root>/src/cs/LionWeb.Integration.WebSocket.Tests/bin/Debug/net8.0
            // (hence 3x ../)
            "node-program-with-error.js",
            "started",
            "RunNodeProgram_With_Error"
        );
        
        Assert.That(_externalProcessRunner.ErrorTriggerEncountered);
    }

    [TearDown]
    public void StopClients()
    {
        _externalProcessRunner.StopAllProcesses();
    }
}
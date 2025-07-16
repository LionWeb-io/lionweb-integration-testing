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

using System.Diagnostics;

namespace LionWeb.Integration.WebSocket.Tests;

/// <summary>
/// Utility class to encapsulate starting external processes.
/// </summary>
public class ExternalProcessRunner
{
    private readonly List<Process> _processes = [];

    /// <summary>
    /// Starts a process (that can be stopped later using <see cref="StopAllProcesses"/>)
    /// from the specified working directory, using the specified executable and arguments.
    /// A process is considered *started* if the specified trigger is encountered on the stdout.
    /// </summary>
    /// <param name="executable"></param>
    /// <param name="workingDirectory"></param>
    /// <param name="arguments"></param>
    /// <param name="trigger"></param>
    public void StartProcess(string executable, string workingDirectory, string arguments, string trigger)
    {
        var process = new Process();

        process.StartInfo.FileName = executable;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.Arguments = arguments.ReplaceLineEndings(" ");
        process.StartInfo.UseShellExecute = false;

        StartProcess(process, trigger);
    }

    public void StartProcess(Process process, string trigger)
    {
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        var processStarted = false;

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data?.Contains(trigger) ?? false)
            {
                processStarted = true;
            }

            Console.WriteLine(args.Data);
        };
        process.ErrorDataReceived += (_, args) => Console.Error.WriteLine(args.Data);

        Assert.That(process.Start());
        _processes.Add(process);
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        while (!processStarted)
        {
            Thread.Sleep(100);
            // TODO  use Task.Delay instead
        }

        Assert.That(!process.HasExited);
    }

    /// <summary>
    /// Stops (/kills) all external processes that have been started using <see cref="StartProcess"/>.
    /// </summary>
    public void StopAllProcesses()
    {
        foreach (var process in _processes)
        {
            if (process.HasExited)
                continue;

            TestContext.WriteLine($"Killing process {process.ProcessName}");
            process.Kill();
        }
    }
}
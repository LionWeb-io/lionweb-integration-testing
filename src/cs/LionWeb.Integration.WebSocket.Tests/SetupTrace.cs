// // Copyright 2024 TRUMPF Laser GmbH
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// // SPDX-FileCopyrightText: 2024 TRUMPF Laser GmbH
// // SPDX-License-Identifier: Apache-2.0

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Diagnostics;

namespace LionWeb.Integration.WebSocket.Tests;

/// Sets defaults for all tests in this and nested namespaces, and enables test output.
/// <remarks>Inspired by https://docs.nunit.org/articles/nunit/technical-notes/usage/Trace-and-Debug-Output.html</remarks>
[SetUpFixture]
// Timeout is deprecated, but the alternative CancelAfter doesn't work in our context. 
[Timeout(TestTimeout * 1000)]
[NonParallelizable]
public class SetupTrace
{
    private const int TestTimeout = 6;  // [s]

    [OneTimeSetUp]
    public void StartTest()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    private const int SetupTimeout = 20;    // [s]

    [OneTimeSetUp]
    [Timeout(SetupTimeout * 1000)]
    public void SetUpTsClient()
    {
        var process = TsClientProcessesExtensions.SetUpTsClient();
        if (process == null)
            return;
        Assert.That(process.Start(), Is.True, process.ToString);
        // wait for 90% of total timeout on setup:
        Assert.That(process.WaitForExit(SetupTimeout * 900), Is.True, process.ToString);
    }

    [OneTimeTearDown]
    public void EndTest()
    {
        Trace.Flush();
    }
}
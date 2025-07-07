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

using LionWeb.Core;
using LionWeb.Core.M3;
using LionWeb.Core.Utilities;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;

namespace LionWeb.Integration.WebSocket.Tests;

public abstract class WebSocketTestBase
{
    private const int TestTimeout = 6000;
    protected const string IpAddress = "localhost";
    protected const int Port = 42424;

    protected readonly LionWebVersions _lionWebVersion;

    protected readonly List<Language> _languages;

    protected WebSocketTestBase(LionWebVersions? lionWebVersion = null, List<Language>? languages = null)
    {
        _lionWebVersion = lionWebVersion ?? LionWebVersions.v2023_1;
        _languages = languages ?? [ShapesLanguage.Instance];
        _languages.AddRange([_lionWebVersion.BuiltIns, _lionWebVersion.LionCore]);
    }

    public TestContext TestContext { get; set; }

    /// We cannot use <see cref="TimeoutAttribute"/>, as it doesn't execute <see cref="TestCleanupAttribute"/>.
    protected void Timeout(Action action)
    {
        try
        {
            Exception? innerException = null;
            var executionTask = Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    innerException = ex;
                    throw;
                }
            });

            // False means execution timed out.
            if (!executionTask.Wait(TestTimeout))
            {
                Assert.Fail("Method exceeded timeout");
            }

            if (innerException != null)
            {
                throw innerException;
            }
        }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1)
        {
            throw ex.InnerExceptions[0];
        }
        
    }

    protected void AssertEquals(INode? a, INode? b) =>
        AssertEquals([a], [b]);

    protected void AssertEquals(IEnumerable<INode?> a, IEnumerable<INode?> b)
    {
        List<IDifference> differences = new Comparer(a.ToList(), b.ToList()).Compare().ToList();
        Assert.IsTrue(differences.Count == 0,
            differences.DescribeAll(new() { LeftDescription = "a", RightDescription = "b" }));
    }
}
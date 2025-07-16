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

using System.Diagnostics;
using LionWeb.Core.M1.Event;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using LionWeb.Integration.WebSocket.Server;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Tests;

public class WebSocketServerTestsWithTsClient : WebSocketServerTestBase
{
    private readonly ExternalProcessRunner _externalProcessRunner = new();

    private void StartTsClient(params string[] tasks)
    {
        _externalProcessRunner.StartProcess(
            "node",
            // Note: the following line means that it's assumed that
            //  1) the lionweb-typescript repo is located right next to the lionweb-integration-testing repo,
            //  2) that the latter repo has been checked out on the delta-protocol-impl branch,
            //  3) and that it's been built entirely successfully.
            $"{Directory.GetCurrentDirectory()}/../../../../../../../lionweb-typescript/packages/deltas-websocket",
                // cwd is assumed to be: <LionWeb dir.>/lionweb-integration-testing/src/cs/LionWeb.Integration.WebSocket.Tests/bin/Debug/net8.0
                // (hence 7x ../)
            $"dist/cli/client.js {Port} {string.Join(",", tasks)}",
            "LionWeb delta protocol client"
        );
    }

    [Test]
    public void SignIn_1()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new Geometry("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartTsClient("SignOn");

        lionWebServer.WaitForReceived(1);
    }

    [Test]
    public void SignIn_2()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new Geometry("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartTsClient("SignOn");
        StartTsClient("SignOn");

        lionWebServer.WaitForReceived(2);
    }

    [Test]
    public void Model()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new Geometry("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartTsClient("SignOn,Wait,SetDocsText");
        StartTsClient("SignOn,AddDocs");

        lionWebServer.WaitForReceived(4);

        AssertEquals(new Geometry("g")
        {
            Documentation = new Documentation("d")
            {
                Text = "hello there"
            }
        }, serverPartition);
    }

    [TearDown]
    public new void StopClients()
    {
        _externalProcessRunner.StopAllProcesses();
    }
}
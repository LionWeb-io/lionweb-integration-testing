﻿// Copyright 2025 LionWeb Project
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
using LionWeb.Core.M1.Event;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using LionWeb.Integration.WebSocket.Server;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Tests;

[TestClass]
public class WebSocketServerTests : WebSocketServerTestBase
{
    [TestMethod]
    public void SignIn_1() => Timeout(() =>
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new Geometry("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", "SignOn");

        lionWebServer.WaitForReceived(1);
    });

    [TestMethod]
    public void SignIn_2() => Timeout(() =>
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new Geometry("a");
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", "SignOn");
        StartClient("B", "SignOn");

        lionWebServer.WaitForReceived(2);
    });

    [TestMethod]
    public void Model() => Timeout(() =>
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new Geometry("a");
        // var serverPartition = new DynamicPartitionInstance("a", ShapesLanguage.Instance.Geometry);
        // var serverPartition = new LenientPartition("a", webSocketServer.LionWebVersion.BuiltIns.Node);
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverPartition, _webSocketServer);

        StartClient("A", "SignOn,Wait,SetDocsText");
        StartClient("B", "SignOn,AddDocs");

        lionWebServer.WaitForReceived(4);

        AssertEquals(new Geometry("g")
        {
            Documentation = new Documentation("d")
            {
                Text = "hello there"
            }
        }, serverPartition);
    });
}
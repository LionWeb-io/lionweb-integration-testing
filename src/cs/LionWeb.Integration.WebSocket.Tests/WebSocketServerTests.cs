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
using LionWeb.Core.M1;
using LionWeb.Core.M1.Event;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Server;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Tests;

[TestFixture(ClientProcesses.CSharp)]
[TestFixture(ClientProcesses.Ts)]
[TestFixture(ClientProcesses.Ts, ClientProcesses.CSharp)]
public class WebSocketServerTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    [Test]
    public void SignIn_1()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new Geometry("a");
        var serverForest = new Forest();
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        serverForest.AddPartitions([serverPartition]);

        StartClient("A", serverPartition.GetType().Name,"SignOn");

        lionWebServer.WaitForReceived(1);
    }

    [Test]
    public void SignIn_2()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new Geometry("a");
        var serverForest = new Forest();
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        serverForest.AddPartitions([serverPartition]);

        StartClient("A", serverPartition.GetType().Name,"SignOn");
        StartClient("B", serverPartition.GetType().Name, "SignOn");

        lionWebServer.WaitForReceived(2);
    }


    [Test]
    public void Model()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        // TODO:
        // Add a new concept to TestLanguage and replace Geometry language with TestLanguage language
        // We miss the following concept in TestLanguage: a concept with a containment which has a property 
        
        var serverPartition = new Geometry("a");
        // var serverPartition = new DynamicPartitionInstance("a", ShapesLanguage.Instance.Geometry);
        // var serverPartition = new LenientPartition("a", webSocketServer.LionWebVersion.BuiltIns.Node);
        var serverForest = new Forest();
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        serverForest.AddPartitions([serverPartition]);

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
    }
    
    [Test]
    public void Partition()
    {
        _webSocketServer = new WebSocketServer(_lionWebVersion) { Languages = _languages };
        _webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new Geometry("a");
        // var serverPartition = new DynamicPartitionInstance("a", ShapesLanguage.Instance.Geometry);
        // var serverPartition = new LenientPartition("a", webSocketServer.LionWebVersion.BuiltIns.Node);
        var serverForest = new Forest();
        Debug.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");

        var lionWebServer =
            new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest, _webSocketServer);

        serverForest.AddPartitions([serverPartition]);

        StartClient("A",  serverPartition.GetType().Name,"SignOn,Wait,SetDocsText");
        StartClient("B",  serverPartition.GetType().Name,"SignOn,AddDocs");

        lionWebServer.WaitForReceived(4);

        AssertEquals(new Geometry("g")
        {
            Documentation = new Documentation("d")
            {
                Text = "hello there"
            }
        }, serverPartition);
    }
}
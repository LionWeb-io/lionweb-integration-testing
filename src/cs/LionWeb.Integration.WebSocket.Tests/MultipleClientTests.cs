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

using LionWeb.Core.M1;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Client;
using LionWeb.Integration.WebSocket.Server;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Tests;

[TestFixture(ClientProcesses.CSharp)]
[TestFixture(ClientProcesses.Ts)]
[TestFixture(ClientProcesses.Ts, ClientProcesses.CSharp)]
public class MultipleClientTests(params ClientProcesses[] clientProcesses) : WebSocketServerTestBase(clientProcesses)
{
    [Test]
    public void SignIn_2()
    {
        _webSocketServer = new WebSocketTestServer(_lionWebVersion, IpAddress, Port, Log) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest,
            _webSocketServer.Connector, Log);

        StartClient("A", Tasks.SignOn);
        StartClient("B", Tasks.SignOn);

        WaitForSent(2);
    }


    [Test]
    public void MultipleClients()
    {
        _webSocketServer = new WebSocketTestServer(_lionWebVersion, IpAddress, Port, Log) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest,
            _webSocketServer.Connector, Log);

        StartClient("A", Tasks.SignOn, Tasks.SubscribeToChangingPartitions, Tasks.Wait, Tasks.Wait, Tasks.AddName_Containment_0_1);
        StartClient("B", Tasks.SignOn, Tasks.AddPartition, Tasks.AddContainment_0_1);

        WaitForSent(6);

        var serverPartition = (TestPartition)serverForest.Partitions.First();

        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
                {
                    Containment_0_1 = new LinkTestConcept("containment_0_1")
                    {
                        Name = "my name"
                    }
                }
            ]
        };
        
        AssertEquals(expected, serverPartition);
    }

    [Test]
    public void Partition()
    {
        _webSocketServer = new WebSocketTestServer(_lionWebVersion, IpAddress, Port, Log) { Languages = _languages };

        var serverForest = new Forest();

        lionWebServer = new LionWebTestRepository(_lionWebVersion, _languages, "server", serverForest,
            _webSocketServer.Connector, Log);

        StartClient("A", Tasks.SignOn, Tasks.AddPartition);
        StartClient("B", Tasks.SignOn);

        WaitForSent(3);

        var serverPartition = (TestPartition)serverForest.Partitions.First();
        
        var expected = new TestPartition("partition")
        {
            Data = new DataTypeTestConcept("data"),
            Links =
            [
                new LinkTestConcept("link")
            ]
        };
        
        AssertEquals(expected, serverPartition);
    }
}
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
using System.Text.Json;
using LionWeb.Core.M1;
using LionWeb.Core.Notification;
using LionWeb.Core.Serialization;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Integration.WebSocket.Client;
using LionWeb.Protocol.Delta;
using LionWeb.Protocol.Delta.Message;
using LionWeb.Protocol.Delta.Message.Event;

namespace LionWeb.Integration.WebSocket.Tests;

public class WebSocketClientTests(ServerProcesses serverProcess) : WebSocketClientTestBase(serverProcess)
{
    [Test]
    public void bla()
    {
        var childAdded = new ChildAdded(
            "parent",
            new DeltaSerializationChunk([
                new SerializedNode()
                {
                    Id = "documentation",
                    Classifier = new MetaPointer("key-Shapes", "1", "key-Documentation"),
                    Properties =
                    [
                        new SerializedProperty()
                        {
                            Property = new MetaPointer("key-Shapes", "1", "key-technical"),
                            Value = null
                        },
                        new SerializedProperty()
                        {
                            Property = new MetaPointer("key-Shapes", "1", "key-text"),
                            Value = null
                        },
                    ],
                    Containments = [],
                    References = [],
                    Annotations = [],
                    Parent = "a"
                }
            ]),
            new MetaPointer("lang", "ver", "key"), 3,
            [new CommandSource("participationId", "commandId")],
            []);

        Console.WriteLine(childAdded);

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            TypeInfoResolver = new DeltaProtocolTypeResolver()
        };
        var serialized = JsonSerializer.Serialize<IDeltaContent>(childAdded, jsonSerializerOptions);

        Console.WriteLine(serialized);

        var deltaEvent = JsonSerializer.Deserialize<IDeltaEvent>(serialized, jsonSerializerOptions);
        Console.WriteLine(deltaEvent);
        Console.WriteLine((deltaEvent as ChildAdded)?.NewChild.Nodes.First());
    }

    [Test]
    public async Task Communication()
    {
        var clientA = new WebSocketTestClient("A", _lionWebVersion, Log);
        clientA.Connector.ReceivedFromRepository += (sender, msg) => Console.WriteLine($"client A received: {msg}");

        var clientB = new WebSocketTestClient("B", _lionWebVersion, Log);
        clientB.Connector.ReceivedFromRepository += (sender, msg) => Console.WriteLine($"client B received: {msg}");

        var ipAddress = "localhost";
        await clientA.ConnectToServer(ipAddress, Port);
        await clientB.ConnectToServer(ipAddress, Port);
        await clientA.Send("hello from client A");
        Thread.Sleep(100);
    }

    [Test]
    public async Task Partition()
    {
        aForest = new Forest();
        aClient = await ConnectWebSocket(aForest, "A", RepositoryId);

        bForest = new Forest();
        bClient = await ConnectWebSocket(bForest, "B", RepositoryId);
        
        var aPartition = new TestPartition("a");
        aForest.AddPartitions([aPartition]);

        WaitForReceived(1);
        
        var bPartition = bForest.Partitions.First() as TestPartition;
        Assert.That(bPartition, Is.Not.Null);

        bPartition!.AddLinks([new LinkTestConcept("containment_0_1")]);
        Debug.WriteLine($"clientB first link {bPartition.Links[0].PrintIdentity()}");

        WaitForReceived(1);

        Debug.WriteLine($"clientA first link {aPartition.Links[0].PrintIdentity()}");
        aPartition.Links[0].Name = "hello there";

        WaitForReceived(1);

        Debug.WriteLine($"clientA Containment_0_1 {aPartition.Links[0].PrintIdentity()}");
        Debug.WriteLine($"clientB Containment_0_1 {bPartition.Links[0].PrintIdentity()}");

        bPartition.Links[0].Name = "bye there";
        WaitForReceived(1);

        AssertEquals(aPartition, bPartition);
    }
}
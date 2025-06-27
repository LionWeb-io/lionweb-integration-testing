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
using LionWeb.Core;
using LionWeb.Core.M1.Event;
using LionWeb.Core.M3;
using LionWeb.Core.Serialization;
using LionWeb.Core.Serialization.Delta;
using LionWeb.Core.Serialization.Delta.Event;
using LionWeb.Core.Serialization.Delta.Query;
using LionWeb.Core.Utilities;
using LionWeb.Integration.Languages;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using LionWeb.Integration.WebSocket.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LionWeb.Integration.WebSocket.Tests;

[TestClass]
public class WebSocketClientTests : WebSocketClientTestBase
{
    [TestMethod]
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
            23,
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

    [TestMethod, Timeout(3000)]
    public async Task Communication()
    {
        var clientA = new WebSocketClient("A");
        clientA.Receive += (sender, msg) => Console.WriteLine($"client A received: {msg}");

        var clientB = new WebSocketClient("B");
        clientB.Receive += (sender, msg) => Console.WriteLine($"client B received: {msg}");

        var ipAddress = "localhost";
        var port = 42424;
        await clientA.ConnectToServer(ipAddress, port);
        await clientB.ConnectToServer(ipAddress, port);
        await clientA.Send("hello from client A");
        Thread.Sleep(100);
    }

    [TestMethod, Timeout(16000)]
    public async Task Model()
    {
        var serverNode = new Geometry("a");

        var aPartition = SameIdCloner.Clone(serverNode);
        var aClient = await ConnectWebSocket(aPartition, "A");

        var bPartition = SameIdCloner.Clone(serverNode);
        var bClient = await ConnectWebSocket(bPartition, "B");

        Debug.WriteLine($"{nameof(aPartition)}: Partition {aPartition.PrintIdentity()}");
        Debug.WriteLine($"{nameof(bPartition)}: Partition {bPartition.PrintIdentity()}");

        bPartition.Documentation = new Documentation("documentation");
        Debug.WriteLine($"clientB Documentation {bPartition.Documentation.PrintIdentity()}");

        aClient.WaitForReplies(1);

        Debug.WriteLine($"clientA Documentation {aPartition.Documentation.PrintIdentity()}");
        aPartition.Documentation.Text = "hello there";

        bClient.WaitForReplies(2);

        Debug.WriteLine($"clientA Documentation {aPartition.Documentation.PrintIdentity()}");
        Debug.WriteLine($"clientB Documentation {bPartition.Documentation.PrintIdentity()}");

        bPartition.Documentation.Text = "bye there";
        aClient.WaitForReplies(2);

        AssertEquals(aPartition, bPartition);
    }
}
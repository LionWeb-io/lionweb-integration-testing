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
using System.Net.WebSockets;
using System.Text;
using LionWeb.Core;
using LionWeb.Core.M1.Event;
using LionWeb.Core.M3;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using LionWeb.Protocol.Delta;
using LionWeb.Protocol.Delta.Client;
using LionWeb.Protocol.Delta.Message;
using LionWeb.Protocol.Delta.Message.Query;

namespace LionWeb.Integration.WebSocket.Client;

public class WebSocketClient(string name) : IDeltaClientConnector
{
    public const int BUFFER_SIZE = 0x10000;

    public const string ClientStartedMessage = "Client started.";

    private static readonly IVersion2023_1 _lionWebVersion = LionWebVersions.v2023_1;

    private static readonly List<Language> _languages =
        [ShapesLanguage.Instance, _lionWebVersion.BuiltIns, _lionWebVersion.LionCore];

    public static async Task Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        string name = args[0];
        string serverIp = args[1];
        int serverPort = int.Parse(args[2]);

        var tasks = args[3].Split(",");

        Debug.WriteLine($"Starting client {name} to connect to {serverIp}:{serverPort}");
        Debug.WriteLine($"{name}: tasks: {string.Join(",", tasks)}");

        var webSocketClient = new WebSocketClient(name);
        var partition = new Geometry("a");
        var lionWeb = new LionWebTestClient(_lionWebVersion, _languages, $"client_{name}", partition, webSocketClient);

        await webSocketClient.ConnectToServer(serverIp, serverPort);

        foreach (var task in tasks)
        {
            switch (task)
            {
                case "SignOn":
                    await webSocketClient.SignOn(lionWeb);
                    break;
                case "SignOff":
                    await webSocketClient.SignOff(lionWeb);
                    break;
                case "AddDocs":
                    partition.Documentation = new Documentation("documentation");
                    lionWeb.WaitForReplies(1);
                    break;
                case "Wait":
                    lionWeb.WaitForReplies(1);
                    break;
                case "SetDocsText":
                    partition.Documentation.Text = "hello there";
                    lionWeb.WaitForReplies(1);
                    break;
            }
        }
        
        Console.ReadLine();
    }

    private readonly EventToDeltaCommandMapper _mapper =
        new(new PartitionEventToDeltaCommandMapper(new CommandIdProvider(), _lionWebVersion));

    private async Task SignOn(LionWebTestClient lionWeb) => 
        await lionWeb.SignOn();


    private async Task SignOff(LionWebTestClient lionWeb) => 
        await lionWeb.SignOff();

    private int nextQueryId = 0;

    private string QueryId() =>
        $"{name}-{nextQueryId++}";

    private readonly DeltaSerializer _deltaSerializer = new();
    private readonly ClientWebSocket _clientWebSocket = new ClientWebSocket();

    /// <inheritdoc />
    public event EventHandler<IDeltaContent>? Receive;

    public async Task ConnectToServer(string ipAddress, int port) =>
        await ConnectToServer($"ws://{ipAddress}:{port}");

    public async Task ConnectToServer(string serverUri)
    {
        await _clientWebSocket.ConnectAsync(new Uri(serverUri), CancellationToken.None);

        Console.WriteLine($"{name}: {ClientStartedMessage} Connected to the server: {serverUri}");

        Task.Run(async () =>
        {
            // Receive messages from the server
            byte[] receiveBuffer = new byte[BUFFER_SIZE];
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result =
                    await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                    // Debug.WriteLine($"XXClient: received message: {receivedMessage}");
                    var deserialized = _deltaSerializer.Deserialize<IDeltaContent>(receivedMessage);
                    // do NOT await
                    Task.Run(() => Receive?.Invoke(this, deserialized));
                    // Debug.WriteLine($"XXClient: processed message: {receivedMessage}");
                }
            }
        });
    }

    /// <inheritdoc />
    public async Task Send(IDeltaContent content) =>
        await Send(_deltaSerializer.Serialize(content));

    public async Task Send(string msg) =>
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text,
            true, CancellationToken.None);

    /// <inheritdoc />
    public IDeltaContent Convert(IEvent internalEvent)
        => _mapper.Map(internalEvent);
}
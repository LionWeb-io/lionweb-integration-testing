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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using LionWeb.Core;
using LionWeb.Core.M1.Event;
using LionWeb.Core.M2;
using LionWeb.Core.M3;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Protocol.Delta;
using LionWeb.Protocol.Delta.Message;
using LionWeb.Protocol.Delta.Message.Event;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Server;

public class WebSocketServer : IDeltaRepositoryConnector
{
    private const int BUFFER_SIZE = 0x10000;
    public const string ServerStartedMessage = "Server started.";

    private static string IpAddress { get; set; } = "localhost";
    private static int Port { get; set; } = 42424;

    public static void Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        Debug.WriteLine($"server args: {string.Join(", ", args)}");

        Concept? optionalTestPartition = args.Length > 0
            ? TestLanguageLanguage.Instance
                .Entities
                .OfType<Concept>()
                .Where(c => c.Partition)
                .FirstOrDefault(p => p.Key == args[0])
            : null;

        LionWebVersions lionWebVersion = LionWebVersions.v2023_1;
        List<Language> languages = optionalTestPartition is not null
            ? [optionalTestPartition.GetLanguage()]
            : [ShapesLanguage.Instance];

        var webSocketServer = new WebSocketServer(lionWebVersion)
        {
            Languages = languages
        };
        webSocketServer.StartServer(IpAddress, Port);

        IPartitionInstance serverPartition = optionalTestPartition is not null
            ? (IPartitionInstance)optionalTestPartition.GetLanguage().GetFactory()
                .CreateNode("partition", optionalTestPartition)
            : new Geometry("a");
        // var serverPartition = new DynamicPartitionInstance("a", ShapesLanguage.Instance.Geometry);
        // var serverPartition = new LenientPartition("a", webSocketServer.LionWebVersion.BuiltIns.Node);
        Debug.WriteLine($"Server partition: <{serverPartition.GetClassifier().Name}>{serverPartition.PrintIdentity()}");

        var lionWebServer = new LionWebRepository(lionWebVersion, webSocketServer.Languages, "server",
            serverPartition,
            webSocketServer);
        Console.ReadLine();
        webSocketServer.Stop();
    }

    public LionWebVersions LionWebVersion;
    public required List<Language> Languages { get; init; }

    private readonly DeltaSerializer _deltaSerializer = new();
    private readonly EventToDeltaEventMapper _mapper;

    private readonly ConcurrentDictionary<IClientInfo, System.Net.WebSockets.WebSocket> _knownClients = [];
    private int _nextParticipationId = 0;

    private HttpListener? _listener;

    public WebSocketServer(LionWebVersions lionWebVersion)
    {
        LionWebVersion = lionWebVersion;
        _mapper = new(new PartitionEventToDeltaEventMapper(new ExceptionParticipationIdProvider(), lionWebVersion));
    }

    /// <inheritdoc />
    public event EventHandler<IMessageContext<IDeltaContent>> Receive;

    public void StartServer(string ipAddress, int port)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{ipAddress}:{port}/");
        _listener.Start();

        Console.WriteLine(ServerStartedMessage + " Waiting for connections...");

        // do NOT await!
        Task.Run(async () =>
        {
            while (true)
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    // do NOT await!
                    ProcessWebSocketRequest(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        });
    }

    public void Stop()
    {
        if (_listener == null)
            return;

        _listener.Stop();
    }

    /// <inheritdoc />
    public async Task SendAll(IDeltaContent content)
    {
        foreach ((var clientInfo, System.Net.WebSockets.WebSocket socket) in _knownClients)
        {
            var encoded = Encode(_deltaSerializer.Serialize(UpdateSequenceNumber(content, clientInfo)));
            await socket.SendAsync(encoded, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private static IDeltaContent UpdateSequenceNumber(IDeltaContent content, IClientInfo clientInfo)
    {
        if (content is IDeltaEvent ev)
        {
            ev.SequenceNumber = clientInfo.GetAndIncrementSequenceNumber();
        }

        return content;
    }

    /// <inheritdoc />
    public IDeltaContent Convert(IEvent internalEvent) =>
        _mapper.Map(internalEvent);

    private static byte[] Encode(string msg) =>
        Encoding.UTF8.GetBytes(msg);


    /// <inheritdoc />
    public async Task Send(IClientInfo clientInfo, IDeltaContent content) =>
        await Send(clientInfo, _deltaSerializer.Serialize(UpdateSequenceNumber(content, clientInfo)));

    private async Task Send(IClientInfo clientInfo, string msg)
    {
        if (_knownClients.TryGetValue(clientInfo, out var socket))
        {
            var encoded = Encode(msg);
            await socket.SendAsync(encoded, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private async Task ProcessWebSocketRequest(HttpListenerContext context)
    {
        HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
        System.Net.WebSockets.WebSocket socket = webSocketContext.WebSocket;
        var clientInfo = new ClientInfo() { ParticipationId = GetNextParticipationId() };
        _knownClients.TryAdd(clientInfo, socket);

        Debug.WriteLine($"WebSocket connection accepted: {context.Request.RemoteEndPoint}");

        // Handle incoming messages
        byte[] buffer = new byte[BUFFER_SIZE];
        while (socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result =
                await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Receive?.Invoke(this,
                        new DeltaMessageContext(clientInfo,
                            _deltaSerializer.Deserialize<IDeltaContent>(receivedMessage)));
                    break;
                }
                case WebSocketMessageType.Close:
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "",
                        CancellationToken.None);
                    _knownClients.TryRemove(clientInfo, out _);
                    break;
            }
        }

        _knownClients.TryRemove(clientInfo, out _);
    }

    private string GetNextParticipationId()
    {
        lock (this)
        {
            return "participation" + _nextParticipationId++;
        }
    }
}
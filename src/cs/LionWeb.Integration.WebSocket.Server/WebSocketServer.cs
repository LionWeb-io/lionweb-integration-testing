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
using LionWeb.Core.M1;
using LionWeb.Core.M2;
using LionWeb.Core.M3;
using LionWeb.Core.Notification;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Protocol.Delta;
using LionWeb.Protocol.Delta.Message;
using LionWeb.Protocol.Delta.Message.Event;
using LionWeb.Protocol.Delta.Repository;

namespace LionWeb.Integration.WebSocket.Server;

public class WebSocketServer : IDeltaRepositoryConnector
{
    private const int BufferSize = 0x10000;
    public const string ServerStartedMessage = "Server started.";

    private static string IpAddress { get; set; } = "localhost";

    public static void Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        Log($"server args: {string.Join(", ", args)}");

        var port = args.Length > 0
            ? int.Parse(args[0])
            : 40000;
        
        var testPartition = args.Length > 1
            ? args[1]
            : throw new ArgumentException("Missing partitionType");

        LionWebVersions lionWebVersion = LionWebVersions.v2023_1;
    List<Language> languages =
        [TestLanguageLanguage.Instance, lionWebVersion.BuiltIns, lionWebVersion.LionCore];

        var webSocketServer = new WebSocketServer(lionWebVersion)
        {
            Languages = languages
        };
        
        webSocketServer.StartServer(IpAddress, port);

        IPartitionInstance serverPartition = languages
            .SelectMany(l => l.Entities)
            .OfType<Concept>()
            .Where(c => c.Partition)
            .Where(c => c.Name == testPartition)
            .Select(c => (IPartitionInstance)c.GetLanguage().GetFactory().CreateNode("a", c))
            .First();
        
        var serverForest = new Forest();
        // var serverPartition = new DynamicPartitionInstance("a", ShapesLanguage.Instance.Geometry);
        // var serverPartition = new LenientPartition("a", webSocketServer.LionWebVersion.BuiltIns.Node);
        Log($"Server partition: <{serverPartition.GetClassifier().Name}>{serverPartition.PrintIdentity()}");

        var lionWebServer = new LionWebRepository(lionWebVersion, webSocketServer.Languages, "server",
            serverForest,
            webSocketServer);

        Console.ReadLine();
        webSocketServer.Stop();
    }

    public LionWebVersions LionWebVersion;
    public required List<Language> Languages { get; init; }

    private readonly DeltaSerializer _deltaSerializer = new();
    private readonly NotificationToDeltaEventMapper _mapper;

    private readonly ConcurrentDictionary<IClientInfo, System.Net.WebSockets.WebSocket> _knownClients = [];
    private int _nextParticipationId = 0;

    private HttpListener? _listener;

    public WebSocketServer(LionWebVersions lionWebVersion)
    {
        LionWebVersion = lionWebVersion;
        _mapper = new(new ExceptionParticipationIdProvider(), lionWebVersion);
    }

    /// <inheritdoc />
    public event EventHandler<IMessageContext<IDeltaContent>>? ReceiveFromClient;

    public void StartServer(string ipAddress, int port)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{ipAddress}:{port}/");
        _listener.Start();

        Log(ServerStartedMessage + " Waiting for connections...");

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
    public async Task SendToAllClients(IDeltaContent content)
    {
        foreach ((var clientInfo, System.Net.WebSockets.WebSocket socket) in _knownClients)
        {
            var encoded = Encode(_deltaSerializer.Serialize(UpdateSequenceNumber(content, clientInfo)));
            // Log($"XXServer: sending to {clientInfo} message: {content.GetType()}");
            await socket.SendAsync(encoded, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private static IDeltaContent UpdateSequenceNumber(IDeltaContent content, IClientInfo clientInfo)
    {
        if (content is IDeltaEvent deltaEvent)
        {
            deltaEvent.SequenceNumber = clientInfo.IncrementAndGetSequenceNumber();
        }

        return content;
    }

    /// <inheritdoc />
    public IDeltaContent Convert(INotification notification) =>
        _mapper.Map(notification);

    private static byte[] Encode(string msg) =>
        Encoding.UTF8.GetBytes(msg);


    /// <inheritdoc />
    public async Task SendToClient(IClientInfo clientInfo, IDeltaContent content) =>
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

        Log($"WebSocket connection accepted: {context.Request.RemoteEndPoint}");

        // Handle incoming messages
        byte[] buffer = new byte[BufferSize];
        while (socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result =
                await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ReceiveFromClient?.Invoke(this,
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
    
    private static void Log(string message, bool header = false) =>
        Console.WriteLine(header
            ? $"{ILionWebRepository.HeaderColor_Start}{message}{ILionWebRepository.HeaderColor_End}"
            : message);
}
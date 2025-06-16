using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using LionWeb.Core;
using LionWeb.Core.M1.Event;
using LionWeb.Core.M3;
using LionWeb.Integration.Languages;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using ParticipationId = string;

namespace LionWeb.Integration.WebSocket.Server;

public class WebSocketServer
{
    public const int BUFFER_SIZE = 0x10000;

    public static async Task Main(string[] args)
    {
        var webSocketServer = new WebSocketServer();
        await webSocketServer.StartServer(IpAddress, Port);

        var serverPartition = new Geometry("a");
        Console.WriteLine($"Server partition: {serverPartition.PrintIdentity()}");
        // serverPartition.Documentation = new Documentation("documentation");

        // var serverPartition = new LenientPartition("serverPartition", server.LionWebVersion.BuiltIns.Node);
        var lionWebServer = new LionWebServer(webSocketServer.LionWebVersion, webSocketServer.Languages, "server", serverPartition,
            async s => await webSocketServer.SendAll(s), async (i, s) => await webSocketServer.Send(i, s));
        webSocketServer.Received += async (sender, msg) => await lionWebServer.Receive(msg);
        Console.ReadLine();
    }

    protected static string IpAddress { get; set; } = "localhost";
    protected static int Port { get; set; } = 42424;

    protected LionWebVersions LionWebVersion { get; init; } = LionWebVersions.v2023_1;
    public List<Language> Languages { get; init; } = [ShapesLanguage.Instance];

    private readonly ConcurrentDictionary<IClientInfo, System.Net.WebSockets.WebSocket> _knownClients = [];
    private int nextParticipationId = 0;

    private HttpListener _listener;

    public event EventHandler<IWebSocketMessage> Received;

    public async Task StartServer(string ipAddress, int port)
    {
        var _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{ipAddress}:{port}/");
        _listener.Start();

        Console.WriteLine("Server started. Waiting for connections...");

        Task.Run(async () =>
        {
            while (true)
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
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

    public async Task SendAll(string msg)
    {
        var encoded = Encode(msg);
        foreach ((_, System.Net.WebSockets.WebSocket socket) in _knownClients)
        {
            await socket.SendAsync(encoded, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private static byte[] Encode(string msg) =>
        Encoding.UTF8.GetBytes(msg);

    public async Task Send(IClientInfo clientInfo, string msg)
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

        Console.WriteLine($"WebSocket connection accepted: {context.Request.RemoteEndPoint}");

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
                    Received?.Invoke(this, new WebSocketMessage(clientInfo, receivedMessage));
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
            return "participation" + nextParticipationId++;
        }
    }
}

internal record WebSocketMessage(IClientInfo ClientInfo, string MessageContent) : IWebSocketMessage;

internal record ClientInfo : IClientInfo
{
    private readonly ParticipationId? _participationId;

    public required ParticipationId ParticipationId
    {
        get => _participationId ?? throw new ArgumentException("ParticipationId not set");
        init => _participationId = value;
    }
}
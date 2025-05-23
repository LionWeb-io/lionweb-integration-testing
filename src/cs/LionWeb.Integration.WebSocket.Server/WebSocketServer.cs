using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using LionWeb.Core;
using LionWeb.Core.M3;
using LionWeb.Integration.Languages;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;

namespace LionWeb.Integration.WebSocket.Server;

public class WebSocketServer
{
    public const int BUFFER_SIZE = 0x10000;

    public static async Task Main(string[] args)
    {
        var server = new WebSocketServer();
        await server.StartServer(IpAddress, Port);
        
        var serverPartition = new Geometry("a");
        // serverPartition.Documentation = new Documentation("documentation");
        
        // var serverPartition = new LenientPartition("serverPartition", server.LionWebVersion.BuiltIns.Node);
        var receiver = new Receiver(server.LionWebVersion, server.Languages, "server", serverPartition, true);
        // receiver.Send(s => server.Send(s));
        server.Received += (sender, msg) => receiver.Receive(msg);
        Console.ReadLine();
    }

    protected static string IpAddress { get; set; } = "localhost";
    protected static int Port { get; set; } = 42424;
    
    protected LionWebVersions LionWebVersion { get; init; } = LionWebVersions.v2023_1;
    public List<Language> Languages { get; init; } = [ShapesLanguage.Instance];

    private readonly ConcurrentDictionary<System.Net.WebSockets.WebSocket, byte> _openSockets = [];
    private HttpListener _listener;

    public event EventHandler<string> Received;
    
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
                } else
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

    public async Task Send(string msg)
    {
        foreach ((System.Net.WebSockets.WebSocket socket, var _) in _openSockets)
        {
            await socket.SendAsync(Encoding.UTF8.GetBytes(msg), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }
    }

    private async Task ProcessWebSocketRequest(HttpListenerContext context)
    {
        HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
        System.Net.WebSockets.WebSocket socket = webSocketContext.WebSocket;
        _openSockets.TryAdd(socket, 1);

        Console.WriteLine($"WebSocket connection accepted: {context.Request.RemoteEndPoint}");

        // Handle incoming messages
        byte[] buffer = new byte[BUFFER_SIZE];
        while (socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result =
                await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Received?.Invoke(this, receivedMessage);
            } else if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "",
                    CancellationToken.None);
                _openSockets.TryRemove(socket, out _);
            }
        }

        _openSockets.TryRemove(socket, out _);
    }
}
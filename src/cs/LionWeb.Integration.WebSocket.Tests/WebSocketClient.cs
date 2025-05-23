using System.Net.WebSockets;
using System.Text;

namespace LionWeb.Integration.WebSocket.Tests;

class WebSocketClient(string name)
{
    public const int BUFFER_SIZE = 0x10000;

    private readonly ClientWebSocket _clientWebSocket = new ClientWebSocket();

    public event EventHandler<string> Received;

    public async Task ConnectToServer(string serverUri)
    {
        await _clientWebSocket.ConnectAsync(new Uri(serverUri), CancellationToken.None);

        Console.WriteLine($"Client {name} Connected to the server: {serverUri}");

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
                    Received?.Invoke(this, receivedMessage);
                }
            }
        });
    }

    public async Task Send(string msg)
    {
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text,
            true, CancellationToken.None);
    }
}
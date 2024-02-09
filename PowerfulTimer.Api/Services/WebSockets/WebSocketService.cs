using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using PowerfulTimer.Api.Services.WebSockets.DTOs;

namespace PowerfulTimer.Api.Services.WebSockets;

public interface IWebSocketService
{
    Task ReceiveMessage<T>(WebSocket socket, Action<WebSocketReceivedMessage<T>> handleMessage);
    Task ReceiveMessage(WebSocket socket, Action<WebSocketReceivedMessage> handleMessage);
    Task Broadcast<T>(T message);
    Task Broadcast(string message);
    void AddSocketConnection(WebSocket webSocket);
    void RemoveSocketConnection(WebSocket webSocket);
}

public class WebSocketService : IWebSocketService
{
    private readonly List<WebSocket> _connections = new List<WebSocket>();

    public async Task ReceiveMessage<T>(WebSocket socket, Action<WebSocketReceivedMessage<T>> handleMessage)
    {
        var buffer = new byte[1024 * 4];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                handleMessage(new WebSocketReceivedMessage<T>(JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(buffer, 0, result.Count))));
            }
            else if (result.MessageType == WebSocketMessageType.Close || socket.State == WebSocketState.Aborted)
            {
                handleMessage(new WebSocketReceivedMessage<T>(result.CloseStatus.Value, result.CloseStatusDescription));
            }
        }
    }

    public async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceivedMessage> handleMessage)
    {
        var buffer = new byte[1024 * 4];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                handleMessage(new WebSocketReceivedMessage(Encoding.UTF8.GetString(buffer, 0, result.Count)));
            }
            else if (result.MessageType == WebSocketMessageType.Close || socket.State == WebSocketState.Aborted)
            {
                handleMessage(new WebSocketReceivedMessage(result.CloseStatus.Value, result.CloseStatusDescription));
            }
        }
    }

    public async Task Broadcast(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        foreach (var socket in _connections)
        {
            if (socket.State == WebSocketState.Open)
            {
                var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    public async Task Broadcast<T>(T message)
    {
        await Broadcast(JsonSerializer.Serialize(message));
    }

    public void AddSocketConnection(WebSocket webSocket)
    {
        _connections.Add(webSocket);
    }

    public void RemoveSocketConnection(WebSocket webSocket)
    {
        _connections.Remove(webSocket);
    }
}

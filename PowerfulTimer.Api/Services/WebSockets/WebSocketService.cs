using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using PowerfulTimer.Api.Services.WebSockets.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.Threading;

namespace PowerfulTimer.Api.Services.WebSockets;

public interface IWebSocketService
{
    long? TimerRemainingSeconds { get; set; }
    Task ReceiveMessage<T>(WebSocket socket, Action<WebSocketReceivedMessage<T>, CancellationToken> handleMessage);
    Task ReceiveMessage(WebSocket socket, Action<WebSocketReceivedMessage, CancellationToken> handleMessage);
    Task Broadcast<T>(T message);
    Task Broadcast(string message);
    void AddSocketConnection(WebSocket webSocket);
    void RemoveSocketConnection(WebSocket webSocket);
    void CancelExistingToken();
}

public class WebSocketService : IWebSocketService
{
    private readonly List<WebSocket> _connections = new List<WebSocket>();
    private CancellationTokenSource _cancellationTokenSource;

    public long? TimerRemainingSeconds { get; set; }

    public async Task ReceiveMessage<T>(WebSocket socket, Action<WebSocketReceivedMessage<T>, CancellationToken> handleMessage)
    {
        var buffer = new byte[1024 * 4];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                _cancellationTokenSource = _cancellationTokenSource ?? new CancellationTokenSource();
                var resultMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (!resultMessage.IsNullOrEmpty())
                {
                    handleMessage(new WebSocketReceivedMessage<T>(JsonSerializer.Deserialize<T>(resultMessage, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    })), _cancellationTokenSource.Token);
                }
            }
            else if (result.MessageType == WebSocketMessageType.Close || socket.State == WebSocketState.Aborted)
            {
                handleMessage(new WebSocketReceivedMessage<T>(result.CloseStatus.Value, result.CloseStatusDescription), CancellationToken.None);
            }
        }
    }

    public async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceivedMessage, CancellationToken> handleMessage)
    {
        var buffer = new byte[1024 * 4];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                _cancellationTokenSource = _cancellationTokenSource ?? new CancellationTokenSource();
                handleMessage(new WebSocketReceivedMessage(Encoding.UTF8.GetString(buffer, 0, result.Count)), _cancellationTokenSource.Token);
            }
            else if (result.MessageType == WebSocketMessageType.Close || socket.State == WebSocketState.Aborted)
            {
                handleMessage(new WebSocketReceivedMessage(result.CloseStatus.Value, result.CloseStatusDescription), CancellationToken.None);
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
        await Broadcast(JsonSerializer.Serialize(message, options: new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    public void AddSocketConnection(WebSocket webSocket)
    {
        _connections.Add(webSocket);
    }

    public void RemoveSocketConnection(WebSocket webSocket)
    {
        _connections.Remove(webSocket);
    }

    public void CancelExistingToken()
    {
        if(_cancellationTokenSource == null)
            return;

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
    }
}

using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using PowerfulTimer.Api.Models;
using PowerfulTimer.Api.Services.WebSockets;
using PowerfulTimer.Api.Services.WebSockets.DTOs;

namespace PowerfulTimer.Api.Controllers;

public class TimerWebSocketController : ControllerBase
{
    private readonly IWebSocketService _websocketService;

    public TimerWebSocketController(IWebSocketService websocketService)
    {
        _websocketService = websocketService;
    }

    [Route("/ws/timer")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task Timer()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _websocketService.AddSocketConnection(webSocket);
            await Handle(webSocket);
        }
        HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    }

    [NonAction]
    private async Task Handle(WebSocket webSocket)
    {
        await _websocketService.ReceiveMessage<TimerWebSocketRequest>(webSocket, async (message, cancellationToken) => await OnMessageReceived(webSocket, message, cancellationToken));
    }

    private async Task OnMessageReceived(WebSocket webSocket, WebSocketReceivedMessage<TimerWebSocketRequest> message, CancellationToken cancellationToken)
    {
        if (message.ClientRequiresClosedConnection)
        {
            _websocketService.RemoveSocketConnection(webSocket);
            await webSocket.CloseAsync(message.CloseStatus, message.Description, CancellationToken.None);
            return;
        }

        if (message.MessageValue.Stop)
        {
            _websocketService.CancelExistingToken();
            _websocketService.TimerRemainingSeconds = message.MessageValue.Seconds;
            var response = message.MessageValue.ToResponse();
            await _websocketService.Broadcast(response);
            return;
        }

        _websocketService.TimerRemainingSeconds = _websocketService.TimerRemainingSeconds ?? message.MessageValue.Seconds;
        if (message.MessageValue.Pause)
        {
            _websocketService.CancelExistingToken();
            var response = message.MessageValue.ToResponse();
            response.InsertTime(TimeSpan.FromSeconds(_websocketService.TimerRemainingSeconds.Value));
            await _websocketService.Broadcast(response);
            return;
        }

        if (message.MessageValue.Play)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                var response = message.MessageValue.ToResponse();
                response.InsertTime(TimeSpan.FromSeconds(_websocketService.TimerRemainingSeconds.Value));
                await _websocketService.Broadcast(response);

                _websocketService.TimerRemainingSeconds--;
                await Task.Delay(1000);
            }
        }
    }
}

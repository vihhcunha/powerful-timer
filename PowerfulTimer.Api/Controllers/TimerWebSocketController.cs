using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using PowerfulTimer.Api.Models;
using PowerfulTimer.Api.Services.WebSockets;

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
    public async Task GetTest()
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
        await _websocketService.ReceiveMessage<TimerWebSocket>(webSocket, async (message) =>
        {
            if (message.ClientRequiresClosedConnection)
            {
                _websocketService.RemoveSocketConnection(webSocket);
                await webSocket.CloseAsync(message.CloseStatus, message.Description, CancellationToken.None);
            }
            else
            {
                await _websocketService.Broadcast<TimerWebSocket>(message.MessageValue);
            }
        });
    }
}

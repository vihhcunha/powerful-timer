using System.Net.WebSockets;

namespace PowerfulTimer.Api.Services.WebSockets.DTOs;

public abstract class WebSocketReceivedMessageBase
{
    public WebSocketCloseStatus CloseStatus { get; }
    public string? Description { get; }
    public bool ClientRequiresClosedConnection { get; set; }

    public WebSocketReceivedMessageBase(WebSocketCloseStatus closeStatus, string? description, bool closeConnection = true)
    {
        CloseStatus = closeStatus;
        Description = description;
        ClientRequiresClosedConnection = closeConnection;
    }

    public WebSocketReceivedMessageBase() { }
}

public class WebSocketReceivedMessage : WebSocketReceivedMessageBase
{
    public string? MessageValue { get; set; }

    public WebSocketReceivedMessage(string messageValue)
    {
        MessageValue = messageValue;
    }

    public WebSocketReceivedMessage(WebSocketCloseStatus closeStatus, string? description, bool closeConnection = true)
        : base(closeStatus, description, closeConnection) { }
}

public class WebSocketReceivedMessage<T> : WebSocketReceivedMessageBase
{
    public T? MessageValue { get; set; }

    public WebSocketReceivedMessage(T messageValue)
    {
        MessageValue = messageValue;
    }

    public WebSocketReceivedMessage(WebSocketCloseStatus closeStatus, string? description, bool closeConnection = true)
        : base(closeStatus, description, closeConnection) { }
}

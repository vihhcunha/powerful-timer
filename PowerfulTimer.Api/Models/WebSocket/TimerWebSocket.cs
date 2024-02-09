namespace PowerfulTimer.Api.Models;

public record TimerWebSocket
{
    public Guid TimerId { get; set; }
    public bool Play { get; set; }
}

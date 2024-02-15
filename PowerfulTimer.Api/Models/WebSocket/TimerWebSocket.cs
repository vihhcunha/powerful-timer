namespace PowerfulTimer.Api.Models;

public record TimerWebSocketRequest
{
    public Guid TimerId { get; set; }
    public bool Play { get; set; }
    public bool Pause { get; set; }
    public bool Stop { get; set; }
    public long Seconds { get; set; }

    public TimerWebSocketResponse ToResponse()
    {
        var response = new TimerWebSocketResponse { TimerId = TimerId, Play = Play, Pause = Pause, Stop = Stop };
        response.InsertTime(TimeSpan.FromSeconds(Seconds));
        return response;
    }
}

public record TimerWebSocketResponse
{
    public Guid TimerId { get; set; }
    public bool Play { get; set; }
    public bool Pause { get; set; }
    public bool Stop { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }

    public void InsertTime(TimeSpan time)
    {
        Hours = time.Hours;
        Minutes = time.Minutes;
        Seconds = time.Seconds;
    }
}

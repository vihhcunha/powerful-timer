namespace PowerfulTimer.Api.Entities;

public class Timer
{
    public Guid TimerId { get; set; }
    public string Name { get; set; }
    public long Seconds { get; set; }
    public int Order { get; set; }

    public Timer()
    {
        TimerId = Guid.NewGuid();
    }
}

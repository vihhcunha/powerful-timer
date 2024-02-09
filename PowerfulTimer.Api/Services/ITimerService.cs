using ErrorOr;
using Timer = PowerfulTimer.Api.Entities.Timer;

namespace PowerfulTimer.Api.Services;

public interface ITimerService
{
    Task<ErrorOr<Success>> AddTimer(string name, long seconds);
    Task<ErrorOr<Success>> EditTimer(Guid id, string name, long seconds);
    Task<ErrorOr<Success>> ReorderTimers(IList<Timer> timers);
    Task<ErrorOr<Success>> DeleteTimer(Guid id);
    Task<ErrorOr<IEnumerable<Timer>>> GetTimers();
}

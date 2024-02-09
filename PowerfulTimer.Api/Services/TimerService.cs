using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PowerfulTimer.Api.Data;
using Timer = PowerfulTimer.Api.Entities.Timer;

namespace PowerfulTimer.Api.Services;

public class TimerService : ITimerService
{
    private readonly PowerfulTimerContext _context;

    public TimerService(PowerfulTimerContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> AddTimer(string name, long seconds)
    {
        var errors = ValidateTimer(name, seconds);

        if (errors.Any())
            return errors;

        var timer = new Timer(name, seconds);

        var timerWithMaxOrder = await _context.Timers.OrderByDescending(x => x.Order).FirstOrDefaultAsync();
        if (timerWithMaxOrder != null)
        {
            timer.Order = timerWithMaxOrder.Order + 1;
        }

        await _context.Timers.AddAsync(timer);
        await _context.SaveChangesAsync();
        return new Success();
    }

    public async Task<ErrorOr<Success>> DeleteTimer(Guid id)
    {
        var existingTimer = _context.Timers.FirstOrDefault(x => x.TimerId == id);
        if (existingTimer == null)
            return new Success();

        _context.Timers.Remove(existingTimer);
        await _context.SaveChangesAsync();
        return new Success();
    }

    public async Task<ErrorOr<Success>> EditTimer(Guid id, string name, long seconds)
    {
        var errors = ValidateTimer(name, seconds);

        if (errors.Any())
            return errors;

        var existingTimer = _context.Timers.FirstOrDefault(x => x.TimerId == id);
        if (existingTimer == null)
            return Error.NotFound(description: "Não foi possível achar o timer que você está editando.");

        existingTimer.Name = name;
        existingTimer.Seconds = seconds;

        await _context.SaveChangesAsync();
        return new Success();
    }

    public async Task<ErrorOr<IEnumerable<Timer>>> GetTimers()
    {
        return await _context.Timers.OrderBy(x => x.Order).AsNoTrackingWithIdentityResolution().ToListAsync();
    }

    public async Task<ErrorOr<Success>> ReorderTimers(IList<Timer> timers)
    {
        var existingTimers = await _context.Timers.ToListAsync();

        foreach (var existingTimer in existingTimers)
        {
            var reorderTimer = timers.FirstOrDefault(x => x.TimerId == existingTimer.TimerId);
            if (reorderTimer == null) return Error.Unexpected(description: "Timers recebidos são diferentes dos timers gravados.");

            existingTimer.Order = reorderTimer.Order;
        }

        await _context.SaveChangesAsync();
        return new Success();
    }

    private List<Error> ValidateTimer(string name, long seconds)
    {
        var errors = new List<Error>();

        if (name.IsNullOrEmpty())
            errors.Add(Error.Validation(description: "É necessário definir um nome para o timer."));

        if (seconds <= 0)
            errors.Add(Error.Validation(description: "É necessário definir um tempo válido."));

        return errors;
    }
}

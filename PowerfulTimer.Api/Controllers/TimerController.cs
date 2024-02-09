using Microsoft.AspNetCore.Mvc;
using PowerfulTimer.Api.Services;
using Timer = PowerfulTimer.Api.Entities.Timer;

namespace PowerfulTimer.Api.Controllers;

public class TimerController : BaseController
{
    private readonly ITimerService _timerService;

    public TimerController(ITimerService timerService)
    {
        _timerService = timerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTimers() => HandleResult(await _timerService.GetTimers());

    [HttpPost]
    public async Task<IActionResult> AddTimers(Timer timer) => HandleResult(await _timerService.AddTimer(timer.Name, timer.Seconds));

    [HttpPut("{timerId}")]
    public async Task<IActionResult> EditTimers(Guid timerId, Timer timer) => HandleResult(await _timerService.EditTimer(timerId, timer.Name, timer.Seconds));

    [HttpPut("reorder")]
    public async Task<IActionResult> ReorderTimers(IList<Timer> timers) => HandleResult(await _timerService.ReorderTimers(timers));

    [HttpDelete("{timerId}")]
    public async Task<IActionResult> DeleteTimers(Guid timerId) => HandleResult(await _timerService.DeleteTimer(timerId));
}
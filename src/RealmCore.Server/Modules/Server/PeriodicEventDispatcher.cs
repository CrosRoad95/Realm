namespace RealmCore.Server.Modules.Server;

public interface IPeriodicEventDispatcher
{
    event Action? EverySecond;
    event Action? EveryMinute;

    internal void DispatchEverySecond();
    internal void DispatchEveryMinute();
}

internal sealed class PeriodicEventDispatcher : IPeriodicEventDispatcher
{
    private readonly ILogger<PeriodicEventDispatcher> _logger;

    public event Action? EverySecond;
    public event Action? EveryMinute;

    public PeriodicEventDispatcher(ILogger<PeriodicEventDispatcher> logger, IScheduler scheduler)
    {
        _logger = logger;

        scheduler.Schedule(DispatchEverySecond).EverySecond().RunOnceAtStart();
        scheduler.Schedule(DispatchEveryMinute).EveryMinute().RunOnceAtStart();
    }

    public void DispatchEverySecond()
    {
        if (EverySecond == null)
            return;

        foreach (var callback in EverySecond.GetInvocationList())
        {
            try
            {
                ((Action)callback)();
            }
            catch (Exception ex)
            {
                _logger.LogHandleError(ex);
            }
        }
    }

    public void DispatchEveryMinute()
    {
        if (EveryMinute == null)
            return;

        foreach (var callback in EveryMinute.GetInvocationList())
        {
            try
            {
                ((Action)callback)();
            }
            catch (Exception ex)
            {
                _logger.LogHandleError(ex);
            }
        }
    }
}

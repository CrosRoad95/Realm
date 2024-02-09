namespace RealmCore.Server.Modules.Server;

public interface IUpdateService
{
    event Action? Update;
    event Action? RareUpdate;

    void InvokeRareUpdate();
    void InvokeUpdate();
}

internal class UpdateService : IUpdateService
{
    private readonly System.Timers.Timer _updateTimer;
    private readonly System.Timers.Timer _rareUpdateTimer;
    private readonly ILogger<UpdateService> _logger;

    public event Action? Update;
    public event Action? RareUpdate;

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;
        _updateTimer = new System.Timers.Timer();
        _updateTimer.Interval = 1000.0f / 20.0f;
        _updateTimer.Elapsed += HandleElapsed;
        _updateTimer.Start();

        _rareUpdateTimer = new System.Timers.Timer();
        _rareUpdateTimer.Interval = 1000;
        _rareUpdateTimer.Elapsed += HandleRareElapsed;
        _rareUpdateTimer.Start();
    }

    public void InvokeUpdate()
    {
        if (Update == null)
            return;

        foreach (var update in Update.GetInvocationList())
        {
            try
            {
                ((Action)update)();
            }
            catch (Exception ex)
            {
                _logger.LogHandleError(ex);
            }
        }
    }

    public void InvokeRareUpdate()
    {
        if (RareUpdate == null)
            return;

        foreach (var rareUpdate in RareUpdate.GetInvocationList())
        {
            try
            {
                ((Action)rareUpdate)();
            }
            catch (Exception ex)
            {
                _logger.LogHandleError(ex);
            }
        }
    }

    private void HandleElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        InvokeUpdate();
    }

    private void HandleRareElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        InvokeRareUpdate();
    }
}

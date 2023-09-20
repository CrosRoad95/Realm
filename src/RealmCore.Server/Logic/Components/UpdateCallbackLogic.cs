namespace RealmCore.Server.Logic.Components;

internal sealed class UpdateCallbackLogic : ComponentLogic<IUpdateCallback>
{
    private readonly System.Timers.Timer _updateTimer;
    private readonly ILogger<UpdateCallbackLogic> _logger;
    private List<IUpdateCallback> _updateCallbacks = new();
    private List<IUpdateCallback> _updateCallbacksToAdd = new();
    private List<IUpdateCallback> _updateCallbacksToRemove = new();
    private object _updateCallbacksLock = new();
    private object _updateCallbacksToAddLock = new();
    private object _updateCallbacksToRemoveLock = new();

    public UpdateCallbackLogic(IEntityEngine entityEngine, ILogger<UpdateCallbackLogic> logger) : base(entityEngine)
    {
        _updateTimer = new System.Timers.Timer(1000.0f/20.0f);
        _updateTimer.Elapsed += HandleElapsed;
        _updateTimer.Start();
        _logger = logger;
    }

    private void HandleElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        lock (_updateCallbacksLock)
        {
            lock (_updateCallbacksToAdd)
            {
                _updateCallbacks.AddRange(_updateCallbacksToAdd);
                _updateCallbacksToAdd.Clear();
            }
        }

        lock (_updateCallbacksLock)
        {
            foreach (var updateCallback in _updateCallbacks)
            {
                try
                {
                    updateCallback.Update();
                }
                catch(Exception ex)
                {
                    _logger.LogHandleError(ex);
                }
            }
        }

        lock (_updateCallbacksLock)
        {
            lock (_updateCallbacksToRemove)
            {
                foreach (var updateCallback in _updateCallbacksToRemove)
                {
                    _updateCallbacks.Remove(updateCallback);
                }
            }
        }
    }

    protected override void ComponentAdded(IUpdateCallback updateCallback)
    {
        lock (_updateCallbacksToAddLock)
            _updateCallbacksToAdd.Add(updateCallback);
    }

    protected override void ComponentDetached(IUpdateCallback component)
    {
        lock(_updateCallbacksToRemoveLock)
            _updateCallbacksToRemove.Add(component);
    }
}

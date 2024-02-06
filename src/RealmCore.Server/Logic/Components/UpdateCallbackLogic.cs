namespace RealmCore.Server.Logic.Components;

internal sealed class UpdateCallbackLogic : ComponentLogic<IUpdateCallback, IRareUpdateCallback>
{
    private readonly System.Timers.Timer _updateTimer;
    private readonly System.Timers.Timer _rareUpdateTimer;
    private readonly ILogger<UpdateCallbackLogic> _logger;
    private readonly IUpdateService _updateService;
    private readonly List<IUpdateCallback> _updateCallbacks = [];
    private readonly List<IUpdateCallback> _updateCallbacksToAdd = [];
    private readonly List<IUpdateCallback> _updateCallbacksToRemove = [];
    private readonly object _updateCallbacksLock = new();
    private readonly object _updateCallbacksToAddLock = new();
    private readonly object _updateCallbacksToRemoveLock = new();

    private readonly List<IRareUpdateCallback> _rareUpdateCallbacks = [];
    private readonly List<IRareUpdateCallback> _rareUpdateCallbacksToAdd = [];
    private readonly List<IRareUpdateCallback> _rareUpdateCallbacksToRemove = [];
    private readonly object _rareUpdateCallbacksLock = new();
    private readonly object _rareUpdateCallbacksToAddLock = new();
    private readonly object _rareUpdateCallbacksToRemoveLock = new();

    public UpdateCallbackLogic(IElementFactory elementFactory, ILogger<UpdateCallbackLogic> logger, IUpdateService updateService) : base(elementFactory)
    {
        _logger = logger;
        _updateService = updateService;
        _updateTimer = new System.Timers.Timer();
        _updateTimer.Interval = 1000.0f / 20.0f;
        _updateTimer.Elapsed += HandleElapsed;
        _updateTimer.Start();

        _rareUpdateTimer = new System.Timers.Timer();
        _rareUpdateTimer.Interval = 1000;
        _rareUpdateTimer.Elapsed += HandleRareElapsed;
        _rareUpdateTimer.Start();
    }

    private void HandleElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            _updateService.RelayUpdate();
        }
        catch(Exception ex)
        {
            _logger.LogHandleError(ex);
        }

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
            lock (_updateCallbacksToRemove)
            {
                foreach (var updateCallback in _updateCallbacksToRemove)
                {
                    _updateCallbacks.Remove(updateCallback);
                }
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
    }

    private void HandleRareElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            _updateService.RelayRareUpdate();
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }

        lock (_rareUpdateCallbacksLock)
        {
            lock (_rareUpdateCallbacksToAdd)
            {
                _rareUpdateCallbacks.AddRange(_rareUpdateCallbacksToAdd);
                _rareUpdateCallbacksToAdd.Clear();
            }
        }

        lock (_rareUpdateCallbacksLock)
        {
            lock (_rareUpdateCallbacksToRemove)
            {
                foreach (var rareUpdateCallback in _rareUpdateCallbacksToRemove)
                {
                    _rareUpdateCallbacks.Remove(rareUpdateCallback);
                }
            }
        }

        lock (_rareUpdateCallbacksLock)
        {
            foreach (var rareUpdateCallback in _rareUpdateCallbacks)
            {
                try
                {
                    rareUpdateCallback.RareUpdate();
                }
                catch (Exception ex)
                {
                    _logger.LogHandleError(ex);
                }
            }
        }
    }

    protected override void ComponentAdded(IUpdateCallback updateCallback)
    {
        lock (_updateCallbacksToAddLock)
            _updateCallbacksToAdd.Add(updateCallback);
    }

    protected override void ComponentDetached(IUpdateCallback updateCallback)
    {
        lock(_updateCallbacksToRemoveLock)
            _updateCallbacksToRemove.Add(updateCallback);
    }

    protected override void ComponentAdded(IRareUpdateCallback rareUpdateCallback)
    {
        lock (_rareUpdateCallbacksToAddLock)
            _rareUpdateCallbacksToAdd.Add(rareUpdateCallback);
    }

    protected override void ComponentDetached(IRareUpdateCallback rareUpdateCallback)
    {
        lock (_rareUpdateCallbacksToRemoveLock)
            _rareUpdateCallbacksToRemove.Add(rareUpdateCallback);
    }
}

namespace RealmCore.Server.Modules.Elements;

internal readonly struct FadeCameraScope : IAsyncDisposable
{
    private readonly RealmPlayer _player;
    private readonly CameraFade _cameraFade;
    private readonly float _fadeTime;
    private readonly CancellationToken _cancellationToken;

    public FadeCameraScope(RealmPlayer player, CameraFade cameraFade, float fadeTime, CancellationToken cancellationToken)
    {
        _player = player;
        _cameraFade = cameraFade;
        _fadeTime = fadeTime;
        _cancellationToken = cancellationToken;
    }

    public async ValueTask DisposeAsync()
    {
        await _player.FadeCameraAsync(_cameraFade, _fadeTime, _cancellationToken);
    }
}

internal abstract record BindHandlerBase(KeyState KeyState);
internal record BindHandler(KeyState KeyState, Action<RealmPlayer, KeyState> Callback) : BindHandlerBase(KeyState);
internal record AsyncBindHandler(KeyState KeyState, Func<RealmPlayer, KeyState, CancellationToken, Task> Callback) : BindHandlerBase(KeyState);

public class RealmPlayer : Player, IAsyncDisposable
{
    private readonly object _lock = new();
    private readonly AtomicBool _inToggleControlScopeFlag = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly AsyncServiceScope _serviceScope;
    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    private Element? _focusedElement;
    private string? _focusedVehiclePart;
    private Element? _lastClickedElement;
    private readonly object _currentInteractElementLock = new();
    private Element? _currentInteractElement;
    private string? _nametagText;
    public virtual Vector2 ScreenSize { get; set; }
    private CultureInfo _culture = new("pl-PL");
    private RealmBlip? _blip = null;

    private readonly Dictionary<string, BindHandlerBase> _binds = [];
    private readonly SemaphoreSlim _bindsLock = new(1);
    private readonly SemaphoreSlim _bindsUpLock = new(1);
    private readonly SemaphoreSlim _bindsDownLock = new(1);
    private readonly object _bindsCooldownLock = new();

    private readonly Dictionary<string, DateTime> _bindsDownCooldown = [];
    private readonly Dictionary<string, DateTime> _bindsUpCooldown = [];
    private readonly ConcurrentDictionary<int, bool> _enableFightFlags = new();
    private readonly List<AttachedBoneWorldObject> _attachedBoneElements = [];
    private readonly ElementBag _selectedElements = new();
    private readonly AsyncRateLimitPolicy _invokePolicy;

    public event Action<RealmPlayer, Element?, Element?>? FocusedElementChanged;
    public event Action<RealmPlayer, string?, string?>? FocusedVehiclePartChanged;
    public event Action<RealmPlayer, Element?>? ClickedElementChanged;
    public event Action<RealmPlayer, Element?, Element?>? CurrentInteractElementChanged;
    public event Action<RealmPlayer, AttachedBoneWorldObject>? WorldObjectAttached;
    public event Action<RealmPlayer, AttachedBoneWorldObject>? WorldObjectDetached;
    public new event Action<RealmPlayer, string?>? NametagTextChanged;
    public event Action<RealmPlayer, bool>? FightEnabled;
    public event Action<RealmPlayer, bool>? EnteredToggleControlScope;

    public List<AttachedBoneWorldObject> AttachedBoneElements => _attachedBoneElements;
    public int AttachedBoneElementsCount => _attachedBoneElements.Count;
    public bool IsSpawned { get; private set; }

    public IServiceProvider ServiceProvider => _serviceProvider;

    public new RealmVehicle? Vehicle => (RealmVehicle?)base.Vehicle;

    public Element? FocusedElement
    {
        get => _focusedElement;
        internal set
        {
            if (value != _focusedElement)
            {
                switch (_focusedElement)
                {
                    case RealmVehicle vehicle:
                        vehicle.RemoveFocusedPlayer(this);
                        break;
                    case FocusableRealmWorldObject worldObject:
                        worldObject.RemoveFocusedPlayer(this);
                        break;
                }

                var previous = _focusedElement;
                _focusedElement = value;
                FocusedElementChanged?.Invoke(this, previous, value);

                switch (value)
                {
                    case RealmVehicle vehicle:
                        vehicle.AddFocusedPlayer(this);
                        break;
                    case FocusableRealmWorldObject worldObject:
                        worldObject.AddFocusedPlayer(this);
                        break;
                }
            }
        }
    }
    
    public string? FocusedVehiclePart
    {
        get => _focusedVehiclePart;
        internal set
        {
            if (value != _focusedVehiclePart)
            {
                var previous = _focusedVehiclePart;
                if(_focusedVehiclePart != value)
                {
                    _focusedVehiclePart = value;
                    FocusedVehiclePartChanged?.Invoke(this, previous, value);
                }
            }
        }
    }

    public Element? LastClickedElement
    {
        get => _lastClickedElement;
        internal set
        {
            if (value != _lastClickedElement)
            {
                _lastClickedElement = value;
                ClickedElementChanged?.Invoke(this, value);
            }
        }
    }

    public Element? CurrentInteractElement
    {
        get => _currentInteractElement; set
        {
            Element? previous = _currentInteractElement;
            bool changed = false;
            lock (_currentInteractElementLock)
            {
                if (value != _currentInteractElement)
                {
                    if (_currentInteractElement != null)
                        _currentInteractElement.Destroyed -= HandleCurrentInteractElementDestroyed;
                    _currentInteractElement = value;
                    if (_currentInteractElement != null)
                        _currentInteractElement.Destroyed += HandleCurrentInteractElementDestroyed;
                    changed = true;
                }
            }
            if (changed)
                CurrentInteractElementChanged?.Invoke(this, previous, value);
        }
    }

    public new string? NametagText
    {
        get => _nametagText;
        set
        {
            _nametagText = value;
            NametagTextChanged?.Invoke(this, _nametagText);
        }
    }

    public CultureInfo Culture { get => _culture; set => _culture = value; }
    public int UserId => User.Id;

    public IElementSaveService Saving { get; init; }
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public new IPlayerMoneyFeature Money { get; init; }
    public IPlayerBrowserFeature Browser { get; init; }
    public IPlayerAFKFeature AFK { get; init; }
    public IPlayerUserFeature User { get; init; }
    public IPlayerDailyVisitsFeature DailyVisits { get; init; }
    public IPlayerSettingsFeature Settings { get; init; }
    public IPlayerBansFeature Bans { get; init; }
    public IPlayerUpgradesFeature Upgrades { get; init; }
    public IPlayerPlayTimeFeature PlayTime { get; init; }
    public IPlayerLevelFeature Level { get; init; }
    public IPlayerLicensesFeature Licenses { get; init; }
    public IPlayerStatisticsFeature Statistics { get; init; }
    public IPlayerAchievementsFeature Achievements { get; init; }
    public IPlayerJobUpgradesFeature JobUpgrades { get; init; }
    public IPlayerDiscoveriesFeature Discoveries { get; init; }
    public IPlayerJobStatisticsFeature JobStatistics { get; init; }
    public IPlayerEventsFeature Events { get; init; }
    public IPlayerSessionsFeature Sessions { get; init; }
    public IPlayerAdminFeature Admin { get; init; }
    public IPlayerGroupsFeature Groups { get; init; }
    public IPlayerFractionsFeature Fractions { get; init; }
    public IPlayerGuiFeature Gui { get; init; }
    public IPlayerHudFeature Hud { get; init; }
    public IPlayerInventoryFeature Inventory { get; init; }
    public IPlayerNotificationsFeature Notifications { get; init; }
    public IPlayerSchedulerFeature Scheduler { get; init; }
    public IPlayerFriendsFeature Friends { get; init; }
    public IPlayerDailyTasksFeature DailyTasks { get; init; }
    public IScopedElementFactory ElementFactory { get; init; }
    public ElementBag SelectedElements => _selectedElements;

    // For test purpuse
    // TODO: Make it better
    public RealmPlayer() { }
    public RealmPlayer(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateAsyncScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        GetRequiredService<PlayerContext>().Player = this;
        GetRequiredService<ElementContext>().Element = this;

        Saving = GetRequiredService<IElementSaveService>();

        #region Initialize scope services
        Money = GetRequiredService<IPlayerMoneyFeature>();
        AFK = GetRequiredService<IPlayerAFKFeature>();
        Browser = GetRequiredService<IPlayerBrowserFeature>();
        User = GetRequiredService<IPlayerUserFeature>();
        DailyVisits = GetRequiredService<IPlayerDailyVisitsFeature>();
        Settings = GetRequiredService<IPlayerSettingsFeature>();
        Bans = GetRequiredService<IPlayerBansFeature>();
        Upgrades = GetRequiredService<IPlayerUpgradesFeature>();
        PlayTime = GetRequiredService<IPlayerPlayTimeFeature>();
        Level = GetRequiredService<IPlayerLevelFeature>();
        Licenses = GetRequiredService<IPlayerLicensesFeature>();
        Statistics = GetRequiredService<IPlayerStatisticsFeature>();
        Achievements = GetRequiredService<IPlayerAchievementsFeature>();
        JobUpgrades = GetRequiredService<IPlayerJobUpgradesFeature>();
        Discoveries = GetRequiredService<IPlayerDiscoveriesFeature>();
        JobStatistics = GetRequiredService<IPlayerJobStatisticsFeature>();
        Events = GetRequiredService<IPlayerEventsFeature>();
        Sessions = GetRequiredService<IPlayerSessionsFeature>();
        Admin = GetRequiredService<IPlayerAdminFeature>();
        Groups = GetRequiredService<IPlayerGroupsFeature>();
        Fractions = GetRequiredService<IPlayerFractionsFeature>();
        Gui = GetRequiredService<IPlayerGuiFeature>();
        Hud = GetRequiredService<IPlayerHudFeature>();
        Inventory = GetRequiredService<IPlayerInventoryFeature>();
        Notifications = GetRequiredService<IPlayerNotificationsFeature>();
        Scheduler = GetRequiredService<IPlayerSchedulerFeature>();
        Friends = GetRequiredService<IPlayerFriendsFeature>();
        DailyTasks = GetRequiredService<IPlayerDailyTasksFeature>();
        ElementFactory = GetRequiredService<IScopedElementFactory>();
        #endregion

        IsNametagShowing = false;
        UpdateFight();
        Wasted += HandleWasted;

        var maxCalls = 10;
        var timeSpan = TimeSpan.FromSeconds(5);
        _invokePolicy = Policy.RateLimitAsync(maxCalls, timeSpan, maxCalls);
    }

    public virtual async Task Invoke(Func<Task> task, CancellationToken cancellationToken = default)
    {
        if (!await _semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken))
            throw new TimeoutException();

        try
        {
            if (User.HasClaim("invokeNoLimit"))
            {
                await task();
                return;
            }

            await _invokePolicy.ExecuteAsync(async () =>
            {
                await task();
            });
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public virtual async Task<T> Invoke<T>(Func<Task<T>> task, CancellationToken cancellationToken = default)
    {
        if (!await _semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken))
            throw new TimeoutException();

        try
        {
            if (User.HasClaim("invokeNoLimit"))
                return await task();

            return await _invokePolicy.ExecuteAsync(async () =>
            {
                return await task();
            });
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private void HandleWasted(Ped sender, PedWastedEventArgs e)
    {
        IsSpawned = false;
    }

    public T GetRequiredService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();
    public object GetRequiredService(Type type) => _serviceProvider.GetRequiredService(type);

    private void UpdateFight()
    {
        var canFight = !_enableFightFlags.IsEmpty;
        Controls.FireEnabled = canFight;
        Controls.AimWeaponEnabled = canFight;
        FightEnabled?.Invoke(this, canFight);
    }

    public bool AddEnableFightFlag(int flag)
    {
        if (_enableFightFlags.TryAdd(flag, true))
        {
            UpdateFight();
            return true;
        }
        return false;
    }

    public bool RemoveEnableFightFlag(int flag)
    {
        if (_enableFightFlags.TryRemove(flag, out var _))
        {
            UpdateFight();
            return true;
        }
        return false;
    }

    public bool TrySpawnAtLastPosition()
    {
        var lastTransformAndMotion = User.UserData.LastTransformAndMotion;
        if (lastTransformAndMotion != null)
        {
            Spawn(lastTransformAndMotion.Position, lastTransformAndMotion.Rotation);
            Interior = lastTransformAndMotion.Interior;
            Dimension = lastTransformAndMotion.Dimension;
            return true;
        }
        return false;
    }

    public void Spawn(Vector3 position, Vector3? rotation = null)
    {
        Spawn(position, rotation?.X ?? 0, 0, 0, 0);
        Camera.Target = this;
        UpdateFight();
        IsSpawned = true;
    }

    public async Task<IAsyncDisposable> FadeCameraAsync(CameraFade cameraFade, float fadeTime = 0.5f, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.CreateCancellationToken());

        Camera.Fade(cameraFade, fadeTime);
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(fadeTime), linkedCancellationToken.Token);
        }
        catch (Exception)
        {
            Camera.Fade(cameraFade == CameraFade.In ? CameraFade.Out : CameraFade.In, 0);
            throw;
        }

        return new FadeCameraScope(this, cameraFade == CameraFade.In ? CameraFade.Out : CameraFade.In, fadeTime, cancellationToken);
    }

    public bool SetBindAsync(string key, Func<RealmPlayer, KeyState, CancellationToken, Task> callback, KeyState keyState = KeyState.Both)
    {
        _bindsLock.Wait();
        try
        {
            if (IsBindInUseCore(key))
                return false;

            SetBind(key, keyState);
            _binds[key] = new AsyncBindHandler(KeyState.Both, callback);
        }
        finally
        {
            _bindsLock.Release();
        }

        return true;
    }

    public void SetBind(string key, Action<RealmPlayer, KeyState> callback, KeyState keyState = KeyState.Both)
    {
        _bindsLock.Wait();
        try
        {
            IsBindInUseCore(key);
            SetBind(key, keyState);
            _binds[key] = new BindHandler(keyState, callback);
        }
        finally
        {
            _bindsLock.Release();
        }
    }

    private bool IsBindInUseCore(string key)
    {
        if (_binds.ContainsKey(key))
        {
            return true;
        }

        return false;
    }

    public bool RemoveBind(string key)
    {
        _bindsLock.Wait();
        try
        {
            if (!!_binds.ContainsKey(key))
            {
                return false;
            }
            RemoveBind(key, KeyState.Both);
            _binds.Remove(key);
        }
        finally
        {
            _bindsLock.Release();
        }

        return true;
    }

    public void RemoveAllBinds()
    {
        _bindsLock.Wait();
        try
        {
            foreach (var pair in _binds)
                RemoveBind(pair.Key, pair.Value.KeyState);

            _binds.Clear();
        }
        finally
        {
            _bindsLock.Release();
        }
    }

    public void ResetCooldown(string key, KeyState keyState = KeyState.Down)
    {
        lock (_bindsCooldownLock)
        {
            if (keyState == KeyState.Down)
                _bindsDownCooldown.Remove(key);
            else
                _bindsUpCooldown.Remove(key);
        }
    }

    internal bool InternalIsCooldownActive(string key, KeyState keyState = KeyState.Down)
    {
        DateTime cooldownUntil = DateTime.MinValue;

        lock (_bindsCooldownLock)
        {
            if (keyState == KeyState.Down)
                _bindsDownCooldown.TryGetValue(key, out cooldownUntil);
            else
                _bindsUpCooldown.TryGetValue(key, out cooldownUntil);
        }

        if (cooldownUntil > DateTime.Now)
        {
            return true;
        }
        return false;
    }

    public bool IsCooldownActive(string key, KeyState keyState = KeyState.Down)
    {
        if (keyState == KeyState.Down)
            _bindsDownLock.Wait();
        else
            _bindsUpLock.Wait();
        try
        {
            return InternalIsCooldownActive(key, keyState);
        }
        finally
        {
            if (keyState == KeyState.Down)
                _bindsDownLock.Release();
            else
                _bindsUpLock.Release();
        }
    }

    internal void SetCooldown(string key, KeyState keyState, DateTime until)
    {
        lock (_bindsCooldownLock)
        {
            if (keyState == KeyState.Down)
                _bindsDownCooldown[key] = until;
            else
                _bindsUpCooldown[key] = until;
        }
    }

    internal void TrySetCooldown(string key, KeyState keyState, DateTime until)
    {
        lock (_bindsCooldownLock)
        {
            if (keyState == KeyState.Down)
            {
                if (_bindsDownCooldown.ContainsKey(key))
                    _bindsDownCooldown[key] = until;
            }
            else
            {
                if (_bindsUpCooldown.ContainsKey(key))
                    _bindsUpCooldown[key] = until;
            }
        }
    }

    internal async Task InternalHandleBindExecuted(string key, KeyState keyState)
    {
        if (IsCooldownActive(key, keyState))
            return;

        // Lock bind indefinitely in case of bind takes a long time to execute, reset cooldown to unlock
        SetCooldown(key, keyState, DateTime.MaxValue);

        if (_binds.TryGetValue(key, out var bindCallback))
        {
            try
            {
                switch (bindCallback)
                {
                    case BindHandler bindHandler:
                        bindHandler.Callback(this, keyState);
                        break;
                    case AsyncBindHandler asyncBindHandler:
                        await asyncBindHandler.Callback(this, keyState, this.CreateCancellationToken());
                        break;
                }
            }
            finally
            {
                TrySetCooldown(key, keyState, DateTime.Now.AddMilliseconds(400));
            }
        }
    }

    public async Task DoComplexAnimationAsync(Animation animation, bool blockMovement = true)
    {
        ToggleControlsScope? scope = null;
        if (blockMovement)
        {
            scope = new ToggleControlsScope(this);
            Controls.WalkEnabled = false;
            Controls.FireEnabled = false;
            Controls.JumpEnabled = false;
        }

        try
        {
            switch (animation)
            {
                case Animation.ComplexLiftUp:
                    SetAnimation("freeweights", "gym_free_pickup", TimeSpan.FromSeconds(3f), false, false, false, false, null, true);
                    await Task.Delay(TimeSpan.FromSeconds(3f));
                    SetAnimation("CARRY", "crry_prtial", TimeSpan.FromSeconds(0.2f), true);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        finally
        {
            scope?.Dispose();
        }
    }

    public void DoAnimation(Animation animation, TimeSpan? timeSpan = null)
    {
        DoAnimationInternal(animation, ref timeSpan);
    }

    internal void DoAnimationInternal(Animation animation, ref TimeSpan? timeSpan)
    {
        switch (animation)
        {
            case Animation.StartCarry:
                timeSpan ??= TimeSpan.FromSeconds(0.1f);
                SetAnimation("CARRY", "crry_prtial", timeSpan, true, false);
                break;
            case Animation.CrouchAndTakeALook:
                timeSpan ??= TimeSpan.FromSeconds(1);
                SetAnimation("COP_AMBIENT", "Copbrowse_nod", timeSpan, true, false);
                break;
            case Animation.Swing:
                timeSpan ??= TimeSpan.FromSeconds(0.5f);
                SetAnimation("SWORD", "sword_block", timeSpan, false, false);
                break;
            case Animation.Click:
                timeSpan ??= TimeSpan.FromSeconds(1);
                SetAnimation("CRIB", "CRIB_Use_Switch", timeSpan, true, false);
                break;
            case Animation.Eat:
                timeSpan ??= TimeSpan.FromSeconds(1);
                SetAnimation("FOOD", "EAT_Burger", timeSpan, true, false);
                break;
            case Animation.Sit:
                timeSpan ??= TimeSpan.FromSeconds(1);
                SetAnimation("BEACH", "ParkSit_M_loop", timeSpan, true, false);
                break;
            case Animation.CarryLiftUp:
                timeSpan ??= TimeSpan.FromSeconds(1.0f);
                SetAnimation("CARRY", "liftup", timeSpan, false, false, false, false);
                break;
            case Animation.CarryPutDown:
                timeSpan ??= TimeSpan.FromSeconds(1.0f);
                SetAnimation("CARRY", "putdwn", timeSpan, false, false, false, false);
                break;
            case Animation.CarryLiftUpFromTable:
                timeSpan ??= TimeSpan.FromSeconds(1.25f);
                SetAnimation("CARRY", "liftup105", timeSpan, false, false, false, false);
                break;
            case Animation.CarryPutDownOnTable:
                timeSpan ??= TimeSpan.FromSeconds(1.0f);
                SetAnimation("CARRY", "putdwn105", timeSpan, false, false, false, false);
                break;
            case Animation.PlantBomb:
                timeSpan ??= TimeSpan.FromSeconds(1.5f);
                SetAnimation("BOMBER", "BOM_Plant", timeSpan, false, false, false, false);
                break;
            case Animation.StartFishing:
                timeSpan ??= TimeSpan.FromSeconds(1.5f);
                SetAnimation("SWORD", "sword_block", timeSpan, false, false, false, true);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    internal bool TryEnterToggleControlScope()
    {
        if (_inToggleControlScopeFlag.TrySetTrue())
        {
            EnteredToggleControlScope?.Invoke(this, true);
            return true;
        }
        return false;
    }

    internal bool ExitToggleControlScope()
    {
        if (_inToggleControlScopeFlag.TrySetFalse())
        {
            EnteredToggleControlScope?.Invoke(this, false);
            return true;
        }
        return false;
    }

    public async Task DoAnimationAsync(Animation animation, TimeSpan? timeSpan = null, bool blockMovement = true, CancellationToken cancellationToken = default)
    {
        ToggleControlsScope? scope = null;
        if (blockMovement)
        {
            scope = new ToggleControlsScope(this);
            Controls.WalkEnabled = false;
            Controls.FireEnabled = false;
            Controls.JumpEnabled = false;
        }

        try
        {
            DoAnimationInternal(animation, ref timeSpan);
            if (timeSpan != null)
                await Task.Delay(timeSpan.Value, cancellationToken);
        }
        finally
        {
            scope?.Dispose();
        }
    }

    public bool Attach(RealmWorldObject worldObject, BoneId boneId, Vector3? positionOffset = null, Vector3? rotationOffset = null)
    {
        lock (_lock)
        {
            if (_attachedBoneElements.Any(x => x.WorldObject == worldObject))
                return false;
            var attachedBoneWorldObject = new AttachedBoneWorldObject(worldObject, boneId, positionOffset, rotationOffset);
            AttachedBoneElements.Add(attachedBoneWorldObject);
            WorldObjectAttached?.Invoke(this, attachedBoneWorldObject);
            worldObject.Destroyed += HandleAttachedWorldObjectDestroyed;
            return true;
        }
    }

    private void HandleAttachedWorldObjectDestroyed(Element element)
    {
        if (element is RealmWorldObject worldObject)
            Detach(worldObject);
    }

    public bool IsAttached(RealmWorldObject worldObject)
    {
        lock (_lock)
            return _attachedBoneElements.Any(x => x.WorldObject == worldObject);
    }

    public bool Detach(RealmWorldObject worldObject)
    {
        lock (_lock)
        {
            var attachedBoneWorldObject = _attachedBoneElements.FirstOrDefault(x => x.WorldObject == worldObject);
            if (attachedBoneWorldObject == null)
                return false;
            if (_attachedBoneElements.Remove(attachedBoneWorldObject))
            {
                worldObject.Destroyed -= HandleAttachedWorldObjectDestroyed;
                WorldObjectDetached?.Invoke(this, attachedBoneWorldObject);
                return true;
            }
            return false;
        }
    }

    public new void WarpIntoVehicle(Vehicle vehicle, byte seat = 0)
    {
        base.WarpIntoVehicle(vehicle, seat);
        if(Vehicle == vehicle)
        {
            Interior = vehicle.Interior;
            Dimension = vehicle.Dimension;
        }
    }

    private void HandleCurrentInteractElementDestroyed(Element _)
    {
        CurrentInteractElement = null;
    }

    public bool AddBlip(Color color)
    {
        if (_blip != null)
            return false;

        _blip = GetRequiredService<IElementFactory>().CreateBlip(this.GetLocation(), BlipIcon.Marker, blip =>
        {
            blip.Color = color;
        });

        PositionChanged += HandlePositionChanged;
        return true;
    }

    private void HandlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        if(_blip != null)
        {
            _blip.Position = Position;
            _blip.Interior = Interior;
            _blip.Dimension = Dimension;
        }
    }

    public bool TryRemoveBlip()
    {
        if (_blip != null)
        {
            PositionChanged -= HandlePositionChanged;
            var result = _blip.Destroy();
            _blip = null;
            return result;
        }
        return false;
    }

    public bool SetBlipColor(Color color)
    {
        if (_blip == null)
            return false;
        _blip.Color = color;
        return true;
    }

    public event Action<RealmPlayer>? Disposed;

    public async ValueTask DisposeAsync()
    {
        Wasted -= HandleWasted;
        TryRemoveBlip();
        await _serviceScope.DisposeAsync();
        IsSpawned = false;
        Disposed?.Invoke(this);
    }
}

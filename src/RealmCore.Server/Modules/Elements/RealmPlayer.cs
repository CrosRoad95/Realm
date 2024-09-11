using RealmCore.Server.Modules.Players.Avatars;
using RealmCore.Server.Modules.Players.Discord;
using RealmCore.Server.Modules.Players.Integrations;
using RealmCore.Server.Modules.Players.Secrets;

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

    public CultureInfo Culture { get => _culture; set => _culture = value; }
    public int UserId => User.Id;

    public ElementSaveService Saving { get; init; }
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public new PlayerMoneyFeature Money { get; init; }
    public PlayerBrowserFeature Browser { get; init; }
    public PlayerAFKFeature AFK { get; init; }
    public PlayerUserFeature User { get; init; }
    public PlayerDailyVisitsFeature DailyVisits { get; init; }
    public PlayerSettingsFeature Settings { get; init; }
    public PlayerBansFeature Bans { get; init; }
    public PlayerUpgradesFeature Upgrades { get; init; }
    public PlayerPlayTimeFeature PlayTime { get; init; }
    public PlayerLevelFeature Level { get; init; }
    public PlayerLicensesFeature Licenses { get; init; }
    public PlayerStatisticsFeature Statistics { get; init; }
    public PlayerAchievementsFeature Achievements { get; init; }
    public PlayerJobUpgradesFeature JobUpgrades { get; init; }
    public PlayerDiscoveriesFeature Discoveries { get; init; }
    public PlayerJobStatisticsFeature JobStatistics { get; init; }
    public PlayerEventsFeature Events { get; init; }
    public PlayerSessionsFeature Sessions { get; init; }
    public PlayerAdminFeature Admin { get; init; }
    public PlayerGroupsFeature Groups { get; init; }
    public PlayerGuiFeature Gui { get; init; }
    public PlayerHudFeature Hud { get; init; }
    public PlayerInventoryFeature Inventory { get; init; }
    public PlayerNotificationsFeature Notifications { get; init; }
    public PlayerSchedulerFeature Scheduler { get; init; }
    public PlayerFriendsFeature Friends { get; init; }
    public PlayerDailyTasksFeature DailyTasks { get; init; }
    public PlayerIntegrationsFeature Integrations { get; init; }
    public PlayerBoostsFeature Boosts { get; init; }
    public PlayerSecretsFeature Secrets { get; init; }
    public DiscordRichPresenceFeature DiscordRichPresence { get; init; }
    public PlayerAvatarFeature Avatar { get; init; }
    public IScopedElementFactory ElementFactory { get; init; }
    public ElementBag SelectedElements => _selectedElements;

    public byte[] DetectedACList { get; private set; } = [];
    public uint? D3D9Size { get; private set; }
    public string? D3D9MD5 { get; private set; }
    public string? D3D9SHA256 { get; private set; }

    public Nametag Nametag { get; init; }

    internal HashSet<int> StartedResources { get; } = [];

    // For test purpuse
    // TODO: Make it better
    public RealmPlayer() { }
    public RealmPlayer(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateAsyncScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        GetRequiredService<PlayerContext>().Player = this;
        GetRequiredService<ElementContext>().Element = this;

        Saving = GetRequiredService<ElementSaveService>();

        #region Initialize scope services
        Money = GetRequiredService<PlayerMoneyFeature>();
        AFK = GetRequiredService<PlayerAFKFeature>();
        Browser = GetRequiredService<PlayerBrowserFeature>();
        User = GetRequiredService<PlayerUserFeature>();
        DailyVisits = GetRequiredService<PlayerDailyVisitsFeature>();
        Settings = GetRequiredService<PlayerSettingsFeature>();
        Bans = GetRequiredService<PlayerBansFeature>();
        Upgrades = GetRequiredService<PlayerUpgradesFeature>();
        PlayTime = GetRequiredService<PlayerPlayTimeFeature>();
        Level = GetRequiredService<PlayerLevelFeature>();
        Licenses = GetRequiredService<PlayerLicensesFeature>();
        Statistics = GetRequiredService<PlayerStatisticsFeature>();
        Achievements = GetRequiredService<PlayerAchievementsFeature>();
        JobUpgrades = GetRequiredService<PlayerJobUpgradesFeature>();
        Discoveries = GetRequiredService<PlayerDiscoveriesFeature>();
        JobStatistics = GetRequiredService<PlayerJobStatisticsFeature>();
        Events = GetRequiredService<PlayerEventsFeature>();
        Sessions = GetRequiredService<PlayerSessionsFeature>();
        Admin = GetRequiredService<PlayerAdminFeature>();
        Groups = GetRequiredService<PlayerGroupsFeature>();
        Gui = GetRequiredService<PlayerGuiFeature>();
        Hud = GetRequiredService<PlayerHudFeature>();
        Inventory = GetRequiredService<PlayerInventoryFeature>();
        Notifications = GetRequiredService<PlayerNotificationsFeature>();
        Scheduler = GetRequiredService<PlayerSchedulerFeature>();
        Friends = GetRequiredService<PlayerFriendsFeature>();
        DailyTasks = GetRequiredService<PlayerDailyTasksFeature>();
        Boosts = GetRequiredService<PlayerBoostsFeature>();
        Integrations = GetRequiredService<PlayerIntegrationsFeature>();
        Secrets = GetRequiredService<PlayerSecretsFeature>();
        DiscordRichPresence = GetRequiredService<DiscordRichPresenceFeature>();
        Avatar = GetRequiredService<PlayerAvatarFeature>();
        ElementFactory = GetRequiredService<IScopedElementFactory>();
        #endregion

        IsNametagShowing = false;
        UpdateFight();

        var maxCalls = 10;
        var timeSpan = TimeSpan.FromSeconds(5);
        _invokePolicy = Policy.RateLimitAsync(maxCalls, timeSpan, maxCalls);
        Nametag = new(this);

        Wasted += HandleWasted;
        ResourceStarted += HandleResourceStarted;
        AcInfoReceived += HandleAcInfoReceived;
    }

    private void HandleAcInfoReceived(Player sender, PlayerACInfoArgs e)
    {
        DetectedACList = e.DetectedACList.ToArray();
        D3D9Size = e.D3D9Size;
        D3D9MD5 = e.D3D9MD5;
        D3D9SHA256 = e.D3D9SHA256;
    }

    private void HandleResourceStarted(Player sender, PlayerResourceStartedEventArgs e)
    {
        StartedResources.Add(e.NetId);
    }

    private AsyncLocal<bool> _invoking = new();
    public virtual async Task Invoke(Func<Task> task, CancellationToken cancellationToken = default)
    {
        if (_invoking.Value)
        {
            await task();
            return;
        }

        _invoking.Value = true;
        try
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
        finally
        {
            _invoking.Value = false;
        }
    }

    public virtual async Task<T> Invoke<T>(Func<Task<T>> task, CancellationToken cancellationToken = default)
    {
        if (_invoking.Value)
            return await task();

        _invoking.Value = true;
        try
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
        finally
        {
            _invoking.Value = false;
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
        var userData = User.UserData;
        var lastTransformAndMotion = userData.LastTransformAndMotion;
        if (lastTransformAndMotion != null)
        {
            Spawn(lastTransformAndMotion.Position, lastTransformAndMotion.Rotation);
            Model = (ushort)userData.Skin;
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
            blip.VisibleDistance = 250;
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
        ResourceStarted -= HandleResourceStarted;
        TryRemoveBlip();
        await _serviceScope.DisposeAsync();
        IsSpawned = false;
        Disposed?.Invoke(this);
    }
}

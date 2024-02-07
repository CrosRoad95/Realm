using RealmCore.Server.Services.Elements;

namespace RealmCore.Server.Elements;

public class RealmPlayer : Player, IDisposable
{
    private readonly object _lock = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;
    public IServiceProvider ServiceProvider => _serviceProvider;

    private Element? _focusedElement;
    private Element? _lastClickedElement;
    private readonly object _currentInteractElementLock = new();
    private Element? _currentInteractElement;
    private string? _nametagText;
    public virtual Vector2 ScreenSize { get; internal set; }
    public virtual CultureInfo CultureInfo { get; internal set; }
    private readonly CultureInfo _culture;
    private readonly Dictionary<string, Func<RealmPlayer, KeyState, CancellationToken, Task>> _asyncBinds = [];
    private readonly Dictionary<string, Action<RealmPlayer, KeyState>> _binds = [];
    private readonly SemaphoreSlim _bindsLock = new(1);
    private readonly SemaphoreSlim _bindsUpLock = new(1);
    private readonly SemaphoreSlim _bindsDownLock = new(1);
    private readonly object _bindsCooldownLock = new();

    private readonly Dictionary<string, DateTime> _bindsDownCooldown = [];
    private readonly Dictionary<string, DateTime> _bindsUpCooldown = [];
    private readonly ConcurrentDictionary<int, bool> _enableFightFlags = new();
    private readonly List<AttachedBoneWorldObject> _attachedBoneElements = [];

    public event Action<RealmPlayer, Element?>? FocusedElementChanged;
    public event Action<RealmPlayer, Element?>? ClickedElementChanged;
    public event Action<RealmPlayer, Element?>? CurrentInteractElementChanged;
    public event Action<RealmPlayer, AttachedBoneWorldObject>? WorldObjectAttached;
    public event Action<RealmPlayer, AttachedBoneWorldObject>? WorldObjectDetached;
    public new event Action<RealmPlayer, string?>? NametagTextChanged;

    public List<AttachedBoneWorldObject> AttachedBoneElements => _attachedBoneElements;
    public int AttachedBoneElementsCount => _attachedBoneElements.Count;

    public Element? FocusedElement
    {
        get => _focusedElement;
        internal set
        {
            if (value != _focusedElement)
            {
                switch (_focusedElement)
                {
                    case FocusableRealmVehicle vehicle:
                        vehicle.RemoveFocusedPlayer(this);
                        break;
                    case FocusableRealmWorldObject worldObject:
                        worldObject.RemoveFocusedPlayer(this);
                        break;
                }

                _focusedElement = value;
                FocusedElementChanged?.Invoke(this, value);

                switch (value)
                {
                    case FocusableRealmVehicle vehicle:
                        vehicle.AddFocusedPlayer(this);
                        break;
                    case FocusableRealmWorldObject worldObject:
                        worldObject.AddFocusedPlayer(this);
                        break;
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
                CurrentInteractElementChanged?.Invoke(this, value);
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

    private void HandleCurrentInteractElementDestroyed(Element _)
    {
        CurrentInteractElement = null;
    }

    public CultureInfo Culture => _culture;
    public bool IsSignedIn => User.IsSignedIn;
    public int UserId => User.Id;

    public new IPlayerMoneyService Money { get; private set; }
    public IPlayerBrowserService Browser { get; private set; }
    public IPlayerAFKService AFK { get; private set; }
    public IPlayerUserService User { get; private set; }
    public IPlayerDailyVisitsService DailyVisits { get; private set; }
    public IPlayerSettingsService Settings { get; private set; }
    public IPlayerBansService Bans { get; private set; }
    public IPlayerUpgradeService Upgrades { get; private set; }
    public IPlayerPlayTimeService PlayTime { get; private set; }
    public IPlayerLevelService Level { get; private set; }
    public IPlayerLicensesService Licenses { get; private set; }
    public IPlayerStatisticsService Statistics { get; private set; }
    public IPlayerAchievementsService Achievements { get; private set; }
    public IPlayerJobUpgradesService JobUpgrades { get; private set; }
    public IPlayerDiscoveriesService Discoveries { get; private set; }
    public IPlayerJobStatisticsService JobStatistics { get; private set; }
    public IPlayerEventsService Events { get; private set; }
    public IPlayerSessionsService Sessions { get; private set; }
    public IPlayerAdminService Admin { get; private set; }
    public IPlayerGroupsService Groups { get; private set; }
    public IPlayerFractionsService Fractions { get; private set; }
    public IPlayerGuiService Gui { get; private set; }
    public IPlayerHudService Hud { get; private set; }
    public IPlayerInventoryService Inventory { get; private set; }
    public IScopedElementFactory ElementFactory { get; private set; }
    public RealmPlayer(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;

        #region Initialize scope services
        GetRequiredService<PlayerContext>().Player = this;
        Money = GetRequiredService<IPlayerMoneyService>();
        AFK = GetRequiredService<IPlayerAFKService>();
        Browser = GetRequiredService<IPlayerBrowserService>();
        User = GetRequiredService<IPlayerUserService>();
        DailyVisits = GetRequiredService<IPlayerDailyVisitsService>();
        Settings = GetRequiredService<IPlayerSettingsService>();
        Bans = GetRequiredService<IPlayerBansService>();
        Upgrades = GetRequiredService<IPlayerUpgradeService>();
        PlayTime = GetRequiredService<IPlayerPlayTimeService>();
        Level = GetRequiredService<IPlayerLevelService>();
        Licenses = GetRequiredService<IPlayerLicensesService>();
        Statistics = GetRequiredService<IPlayerStatisticsService>();
        Achievements = GetRequiredService<IPlayerAchievementsService>();
        JobUpgrades = GetRequiredService<IPlayerJobUpgradesService>();
        Discoveries = GetRequiredService<IPlayerDiscoveriesService>();
        JobStatistics = GetRequiredService<IPlayerJobStatisticsService>();
        Events = GetRequiredService<IPlayerEventsService>();
        Sessions = GetRequiredService<IPlayerSessionsService>();
        Admin = GetRequiredService<IPlayerAdminService>();
        Groups = GetRequiredService<IPlayerGroupsService>();
        Fractions = GetRequiredService<IPlayerFractionsService>();
        Gui = GetRequiredService<IPlayerGuiService>();
        Hud = GetRequiredService<IPlayerHudService>();
        Inventory = GetRequiredService<IPlayerInventoryService>();
        ElementFactory = GetRequiredService<IScopedElementFactory>();
        #endregion

        BindExecuted += HandleBindExecuted;
        IsNametagShowing = false;
        UpdateFight();
    }

    public T GetRequiredService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();

    public object GetRequiredService(Type type) => _serviceProvider.GetRequiredService(type);

    private void UpdateFight()
    {
        var canFight = !_enableFightFlags.IsEmpty;
        Controls.FireEnabled = canFight;
        Controls.AimWeaponEnabled = canFight;
        GetRequiredService<ILogger>().LogInformation("Can fight {canFight}", canFight);
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
        var lastTransformAndMotion = User.LastTransformAndMotion;
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
    }

    public async Task FadeCameraAsync(CameraFade cameraFade, float fadeTime = 0.5f, CancellationToken cancellationToken = default)
    {
        var linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancellationToken);

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
    }

    public void SetBindAsync(string key, Func<RealmPlayer, KeyState, CancellationToken, Task> callback)
    {
        _bindsLock.Wait();
        if (_asyncBinds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindAlreadyExistsException(key);
        }

        SetBind(key, KeyState.Both);
        _asyncBinds[key] = callback;
        _bindsLock.Release();
    }

    public void SetBindAsync(string key, Func<RealmPlayer, CancellationToken, Task> callback)
    {
        _bindsLock.Wait();
        if (_asyncBinds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindAlreadyExistsException(key);
        }

        SetBind(key, KeyState.Down);
        _asyncBinds[key] = async (player, keyState, cancellationToken) =>
        {
            if (keyState == KeyState.Down)
                await callback(this, cancellationToken);
        };
        _bindsLock.Release();
    }

    public void SetBind(string key, Action<RealmPlayer, KeyState> callback)
    {
        _bindsLock.Wait();
        if (_asyncBinds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindAlreadyExistsException(key);
        }

        SetBind(key, KeyState.Both);
        _binds[key] = callback;
        _bindsLock.Release();
    }

    public void SetBind(string key, Action<RealmPlayer> callback)
    {
        _bindsLock.Wait();
        if (_binds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindAlreadyExistsException(key);
        }

        SetBind(key, KeyState.Down);
        _binds[key] = (player, keyState) =>
        {
            if (keyState == KeyState.Down)
                callback(this);
        };
        _bindsLock.Release();
    }

    public void Unbind(string key)
    {
        _bindsLock.Wait();
        if (!_asyncBinds.ContainsKey(key) || !_binds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindDoesNotExistsException(key);
        }
        RemoveBind(key, KeyState.Both);
        _asyncBinds.Remove(key);
        _binds.Remove(key);

        _bindsLock.Release();
    }

    public void RemoveAllBinds()
    {
        _bindsLock.Wait();
        foreach (var pair in _asyncBinds)
            RemoveBind(pair.Key, KeyState.Both);
        foreach (var pair in _binds)
            RemoveBind(pair.Key, KeyState.Both);

        _asyncBinds.Clear();
        _binds.Clear();

        _bindsLock.Release();
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

    private async void HandleBindExecuted(Player _, PlayerBindExecutedEventArgs e)
    {
        try
        {
            await InternalHandleBindExecuted(e.Key, e.KeyState);
        }
        catch (Exception)
        {
            // TODO: handle exception
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

        // Lock bind indefinitly in case of bind takes a long time to execute, reset cooldown to unlock
        SetCooldown(key, keyState, DateTime.MaxValue);


        if (_binds.TryGetValue(key, out var bindCallback))
        {
            try
            {
                bindCallback(this, keyState);
            }
            finally
            {
                TrySetCooldown(key, keyState, DateTime.Now.AddMilliseconds(400));
            }
        }

        if (_asyncBinds.TryGetValue(key, out var asyncBindCallback))
        {
            try
            {
                await asyncBindCallback(this, keyState, CancellationToken);
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
        catch (Exception)
        {
            throw;
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
                timeSpan ??= TimeSpan.FromSeconds(1);
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

    private int _inToggleControlScopeFlag = 0;

    internal bool TryEnterToggleControlScope()
    {
        return Interlocked.Exchange(ref _inToggleControlScopeFlag, 1) == 0;
    }

    internal void ExitToggleControlScope()
    {
        Interlocked.Exchange(ref _inToggleControlScopeFlag, 0);
    }

    public async Task DoAnimationAsync(Animation animation, TimeSpan? timeSpan = null, bool blockMovement = true)
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
                await Task.Delay(timeSpan.Value);
        }
        catch (Exception)
        {
            throw;
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
        if(element is RealmWorldObject realmWorldObject)
            Detach(realmWorldObject);
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
                WorldObjectAttached?.Invoke(this, attachedBoneWorldObject);
                return true;
            }
            return false;
        }
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}

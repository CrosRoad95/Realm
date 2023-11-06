namespace RealmCore.Server.Elements;

public class RealmPlayer : Player, IComponents, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;

    private CancellationTokenSource _cancellationTokenSource = new();
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;
    public IServiceProvider ServiceProvider => _serviceProvider;
    public IScopedElementFactory ElementFactory => GetRequiredService<IScopedElementFactory>();
    public Concepts.Components Components { get; private set; }

    private Element? _focusedElement;
    private Element? _lastClickedElement;
    public virtual Vector2 ScreenSize { get; internal set; }
    public virtual CultureInfo CultureInfo { get; internal set; }
    private readonly CultureInfo _culture;
    private readonly Dictionary<string, Func<RealmPlayer, KeyState, Task>> _asyncBinds = new();
    private readonly Dictionary<string, Action<RealmPlayer, KeyState>> _binds = new();
    private readonly SemaphoreSlim _bindsLock = new(1);
    private readonly SemaphoreSlim _bindsUpLock = new(1);
    private readonly SemaphoreSlim _bindsDownLock = new(1);
    private readonly object _bindsCooldownLock = new();

    private readonly Dictionary<string, DateTime> _bindsDownCooldown = new();
    private readonly Dictionary<string, DateTime> _bindsUpCooldown = new();
    private readonly ConcurrentDictionary<int, bool> _enableFightFlags = new();

    public event Action<RealmPlayer, Element?>? FocusedElementChanged;
    public event Action<RealmPlayer, Element?>? ClickedElementChanged;

    public Element? FocusedElement
    {
        get => _focusedElement;
        internal set
        {
            if (value != _focusedElement)
            {
                _focusedElement = value;
                FocusedElementChanged?.Invoke(this, value);
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

    public CultureInfo Culture => _culture;
    public bool IsSignedIn => User.IsSignedIn;
    public int UserId => User.Id;

    public new IPlayerMoneyService Money => _serviceProvider.GetRequiredService<IPlayerMoneyService>();
    public IPlayerAFKService AFK => _serviceProvider.GetRequiredService<IPlayerAFKService>();
    public IPlayerUserService User => _serviceProvider.GetRequiredService<IPlayerUserService>();
    public RealmPlayer(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        #region Initialize scope services
        _serviceProvider.GetRequiredService<PlayerContext>().Player = this;
        _serviceProvider.GetRequiredService<IPlayerBrowserService>();
        #endregion

        Components = new(_serviceProvider, this);
        BindExecuted += HandleBindExecuted;
        IsNametagShowing = false;
        UpdateFight();
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public TComponent GetRequiredComponent<TComponent>() where TComponent : IComponent
    {
        return Components.GetRequiredComponent<TComponent>();
    }

    public bool TryDestroyComponent<TComponent>() where TComponent : IComponent
    {
        return Components.TryDestroyComponent<TComponent>();
    }

    public void DestroyComponent<TComponent>() where TComponent : IComponent
    {
        Components.DestroyComponent<TComponent>();
    }
    
    public void DestroyComponent<TComponent>(TComponent component) where TComponent : IComponent
    {
        Components.DestroyComponent(component);
    }

    public bool TryGetComponent<TComponent>(out TComponent component) where TComponent : IComponent
    {
        if(Components.TryGetComponent(out TComponent tempComponent))
        {
            component = tempComponent;
            return true;
        }
        component = default!;
        return false;
    }

    public bool HasComponent<TComponent>() where TComponent : IComponent
    {
        return Components.HasComponent<TComponent>();
    }

    public TComponent AddComponent<TComponent>() where TComponent : IComponent, new()
    {
        return Components.AddComponent<TComponent>();
    }

    public TComponent AddComponent<TComponent>(TComponent component) where TComponent : IComponent
    {
        return Components.AddComponent(component);
    }
    
    public TComponent AddComponentWithDI<TComponent>(params object[] parameters) where TComponent : IComponent
    {
        return Components.AddComponentWithDI<TComponent>(parameters);
    }

    private void UpdateFight()
    {
        var canFight = _enableFightFlags.Count != 0;
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

    public void SetBindAsync(string key, Func<RealmPlayer, KeyState, Task> callback)
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

    public void SetBindAsync(string key, Func<RealmPlayer, Task> callback)
    {
        _bindsLock.Wait();
        if (_asyncBinds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindAlreadyExistsException(key);
        }

        SetBind(key, KeyState.Down);
        _asyncBinds[key] = async (player, keyState) =>
        {
            if (keyState == KeyState.Down)
                await callback(this);
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
        if (_asyncBinds.ContainsKey(key))
            _asyncBinds.Remove(key);
        if (_binds.ContainsKey(key))
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
                await asyncBindCallback(this, keyState);
            }
            finally
            {
                TrySetCooldown(key, keyState, DateTime.Now.AddMilliseconds(400));
            }
        }
    }

    public async Task DoComplexAnimationAsync(Animation animation, bool blockMovement = true)
    {
        using var scope = new ToggleControlsScope(this);
        if (blockMovement)
        {
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
            scope.Dispose();
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

    public async Task DoAnimationAsync(Animation animation, TimeSpan? timeSpan = null, bool blockMovement = true)
    {
        using var scope = new ToggleControlsScope(this);
        if (blockMovement)
        {
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
            scope.Dispose();
        }
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        Components.Dispose();
    }
}

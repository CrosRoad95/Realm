using HudComponent = SlipeServer.Server.Elements.Enums.HudComponent;

namespace RealmCore.Server.Components.Elements;

public sealed class PlayerElementComponent : PedElementComponent
{
    [Inject]
    private IECS ECS { get; set; } = default!;
    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;
    [Inject]
    private IOverlayService OverlayService { get; set; } = default!;
    [Inject]
    private IClientInterfaceService ClientInterfaceService { get; set; } = default!;
    [Inject]
    private IGuiSystemService GuiSystemService { get; set; } = default!;
    [Inject]
    private Text3dService Text3dService { get; set; } = default!;
    [Inject]
    private ILogger<PlayerElementComponent> Logger { get; set; } = default!;

    private Entity? _focusedEntity;
    private readonly Player _player;
    private readonly Vector2 _screenSize;
    private readonly CultureInfo _culture;
    private readonly Dictionary<string, Func<Entity, KeyState, Task>> _asyncBinds = new();
    private readonly Dictionary<string, Action<Entity, KeyState>> _binds = new();
    private readonly SemaphoreSlim _bindsLock = new(1);
    private readonly SemaphoreSlim _bindsUpLock = new(1);
    private readonly SemaphoreSlim _bindsDownLock = new(1);
    private readonly object _bindsCooldownLock = new();

    private readonly Dictionary<string, DateTime> _bindsDownCooldown = new();
    private readonly Dictionary<string, DateTime> _bindsUpCooldown = new();
    private readonly List<short> _enableFightFlags = new();
    private readonly object _enableFightFlagsLock = new();
    private readonly MapIdGenerator _mapIdGenerator = new(IdGeneratorConstants.MapIdStart, IdGeneratorConstants.MapIdStop);
    public event Action<Entity, Entity?>? FocusedEntityChanged;
    internal Player Player => _player;
    internal bool Spawned { get; set; }

    public Entity? FocusedEntity
    {
        get
        {
            ThrowIfDisposed();
            return _focusedEntity;
        }
        internal set
        {
            ThrowIfDisposed();
            if (value != _focusedEntity)
            {
                _focusedEntity = value;
                FocusedEntityChanged?.Invoke(Entity, value);
            }
        }
    }

    public Vector2 ScreenSize
    {
        get
        {
            ThrowIfDisposed();
            return _screenSize;
        }
    }

    public CultureInfo Culture
    {
        get
        {
            ThrowIfDisposed();
            return _culture;
        }
    }

    public string Name
    {
        get
        {
            ThrowIfDisposed();
            return _player.Name;
        }
        set
        {
            ThrowIfDisposed();
            _player.Name = value;
        }
    }

    public byte WantedLevel
    {
        get
        {
            ThrowIfDisposed();
            return _player.WantedLevel;
        }
        set
        {
            ThrowIfDisposed();
            _player.WantedLevel = value;
        }
    }

    public Vector3 AimOrigin
    {
        get
        {
            ThrowIfDisposed();
            return _player.AimOrigin;
        }
        set
        {
            ThrowIfDisposed();
            _player.AimOrigin = value;
        }
    }

    public Vector3 AimDirection
    {
        get
        {
            ThrowIfDisposed();
            return _player.AimDirection;
        }
        set
        {
            ThrowIfDisposed();
            _player.AimDirection = value;
        }
    }

    public Vector3 CameraPosition
    {
        get
        {
            ThrowIfDisposed();
            return _player.CameraPosition;
        }
        set
        {
            ThrowIfDisposed();
            _player.CameraPosition = value;
        }
    }

    public Vector3 CameraDirection
    {
        get
        {
            ThrowIfDisposed();
            return _player.CameraDirection;
        }
        set
        {
            ThrowIfDisposed();
            _player.CameraDirection = value;
        }
    }

    public float CameraRotation
    {
        get
        {
            ThrowIfDisposed();
            return _player.CameraRotation;
        }
        set
        {
            ThrowIfDisposed();
            _player.CameraRotation = value;
        }
    }

    public bool IsOnGround
    {
        get
        {
            ThrowIfDisposed();
            return _player.IsOnGround;
        }
        set
        {
            ThrowIfDisposed();
            _player.IsOnGround = value;
        }
    }

    public bool WearsGoggles
    {
        get
        {
            ThrowIfDisposed();
            return _player.WearsGoggles;
        }
        set
        {
            ThrowIfDisposed();
            _player.WearsGoggles = value;
        }
    }

    public bool HasContact
    {
        get
        {
            ThrowIfDisposed();
            return _player.HasContact;
        }
        set
        {
            ThrowIfDisposed();
            _player.HasContact = value;
        }
    }

    public bool IsChoking
    {
        get
        {
            ThrowIfDisposed();
            return _player.IsChoking;
        }
        set
        {
            ThrowIfDisposed();
            _player.IsChoking = value;
        }
    }

    public IClient Client
    {
        get
        {
            ThrowIfDisposed();
            return _player.Client;
        }
    }

    public Controls Controls
    {
        get
        {
            ThrowIfDisposed();
            return _player.Controls;
        }
    }
    
    public bool IsInWater
    {
        get
        {
            ThrowIfDisposed();
            return _player.IsInWater;
        }
    }

    internal override Element Element => _player;
    internal MapIdGenerator MapIdGenerator => _mapIdGenerator;

    public event Action<PlayerElementComponent, PedWastedEventArgs>? Wasted;
    public event Action<PlayerElementComponent, PlayerDamagedEventArgs>? Damaged;

    public Entity? OccupiedVehicle
    {
        get
        {
            ThrowIfDisposed();
            if (_player.Vehicle == null)
                return null;

            ECS.TryGetByElement(_player.Vehicle, out var entity);
            return entity;
        }
    }

    internal PlayerElementComponent(Player player, Vector2 screenSize, CultureInfo cultureInfo) : base(player)
    {
        _player = player;
        _screenSize = screenSize;
        _culture = cultureInfo;
    }

    protected override void Load()
    {
        ThrowIfDisposed();
        Entity.Transform.Bind(_player);
        _player.BindExecuted += HandleBindExecuted;
        _player.Wasted += HandleWasted;
        _player.Damaged += HandleDamaged;
        UpdateFight();
        _player.IsNametagShowing = false;
    }

    private void HandleDamaged(Player sender, PlayerDamagedEventArgs e)
    {
        Damaged?.Invoke(this, e);
    }

    private void HandleWasted(Ped sender, PedWastedEventArgs e)
    {
        Wasted?.Invoke(this, e);
    }

    private void UpdateFight()
    {
        _player.Controls.FireEnabled = _enableFightFlags.Any();
    }

    public bool AddEnableFightFlag(short flag)
    {
        ThrowIfDisposed();
        lock (_enableFightFlagsLock)
        {
            if (!_enableFightFlags.Contains(flag))
            {
                _enableFightFlags.Add(flag);
                UpdateFight();
                return true;
            }
        }
        return false;
    }

    public bool RemoveEnableFightFlag(short flag)
    {
        ThrowIfDisposed();
        lock (_enableFightFlagsLock)
        {
            if (_enableFightFlags.Contains(flag))
            {
                _enableFightFlags.Remove(flag);
                UpdateFight();
                return true;
            }
        }
        return false;
    }

    public bool Compare(Player player) => _player == player;

    public bool TrySpawnAtLastPosition()
    {
        ThrowIfDisposed();
        var userComponent = Entity.GetComponent<UserComponent>();
        if (userComponent != null)
        {
            var lastTransformAndMotion = userComponent.User.LastTransformAndMotion;
            if (lastTransformAndMotion != null)
            {
                Spawn(lastTransformAndMotion.Position, lastTransformAndMotion.Rotation);
                Entity.Transform.Interior = lastTransformAndMotion.Interior;
                Entity.Transform.Dimension = lastTransformAndMotion.Dimension;
                return true;
            }
        }

        return false;
    }

    public void Spawn(Vector3 position, Vector3? rotation = null)
    {
        ThrowIfDisposed();
        _player.Camera.Target = _player;
        _player.Spawn(position, 0, 0, 0, 0);
        Entity.Transform.Position = position;
        Entity.Transform.Rotation = rotation ?? Vector3.Zero;
        Spawned = true;
    }

    public void SetCameraMatrix(Vector3 from, Vector3 to)
    {
        ThrowIfDisposed();
        _player.Camera.SetMatrix(from, to);
    }

    public void FadeCamera(CameraFade cameraFade, float fadeTime = 0.5f)
    {
        ThrowIfDisposed();
        _player.Camera.Fade(cameraFade, fadeTime);
    }

    public Task FadeCameraAsync(CameraFade cameraFade, float fadeTime = 0.5f)
    {
        ThrowIfDisposed();
        _player.Camera.Fade(cameraFade, fadeTime);
        return Task.Delay(TimeSpan.FromSeconds(fadeTime));
    }

    public void SetCameraTarget(Entity entity)
    {
        ThrowIfDisposed();
        var elementComponent = entity.GetRequiredComponent<ElementComponent>();
        _player.Camera.Target = elementComponent.Element;
    }

    #region ClientInterface resource
    public void SetClipboard(string content)
    {
        ThrowIfDisposed();
        ClientInterfaceService.SetClipboard(_player, content);
    }
    #endregion

    #region Overlay resource
    public void AddNotification(string message)
    {
        ThrowIfDisposed();
        OverlayService.AddNotification(_player, message);
    }
    #endregion

    public void SetBindAsync(string key, Func<Entity, KeyState, Task> callback)
    {
        ThrowIfDisposed();

        _bindsLock.Wait();
        if (_asyncBinds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindAlreadyExistsException(key);
        }

        _player.SetBind(key, KeyState.Both);
        _asyncBinds[key] = callback;
        _bindsLock.Release();
    }

    public void SetBindAsync(string key, Func<Entity, Task> callback)
    {
        ThrowIfDisposed();
        _bindsLock.Wait();
        if (_asyncBinds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindAlreadyExistsException(key);
        }

        _player.SetBind(key, KeyState.Down);
        _asyncBinds[key] = async (entity, keyState) =>
        {
            if (keyState == KeyState.Down)
                await callback(entity);
        };
        _bindsLock.Release();
    }

    public void SetBind(string key, Action<Entity, KeyState> callback)
    {
        ThrowIfDisposed();

        _bindsLock.Wait();
        if (_asyncBinds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindAlreadyExistsException(key);
        }

        _player.SetBind(key, KeyState.Both);
        _binds[key] = callback;
        _bindsLock.Release();
    }

    public void SetBind(string key, Action<Entity> callback)
    {
        ThrowIfDisposed();
        _bindsLock.Wait();
        if (_binds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindAlreadyExistsException(key);
        }

        _player.SetBind(key, KeyState.Down);
        _binds[key] = (entity, keyState) =>
        {
            if (keyState == KeyState.Down)
                callback(entity);
        };
        _bindsLock.Release();
    }

    public void Unbind(string key)
    {
        ThrowIfDisposed();
        _bindsLock.Wait();
        if (!_asyncBinds.ContainsKey(key) || !_binds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindDoesNotExistsException(key);
        }
        _player.RemoveBind(key, KeyState.Both);
        if (_asyncBinds.ContainsKey(key))
            _asyncBinds.Remove(key);
        if (_binds.ContainsKey(key))
            _binds.Remove(key);
        _bindsLock.Release();
    }

    public void ResetCooldown(string key, KeyState keyState = KeyState.Down)
    {
        ThrowIfDisposed();

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
        catch(Exception ex)
        {
            Logger.LogHandleError(ex);
        }
        finally
        {

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

        if (cooldownUntil > DateTimeProvider.Now)
        {
            return true;
        }
        return false;
    }

    public bool IsCooldownActive(string key, KeyState keyState = KeyState.Down)
    {
        ThrowIfDisposed();

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
        ThrowIfDisposed();

        if (IsCooldownActive(key, keyState))
            return;

        // Lock bind indefinitly in case of bind takes a long time to execute, reset cooldown to unlock
        SetCooldown(key, keyState, DateTime.MaxValue);


        if (_binds.TryGetValue(key, out var bindCallback))
        {
            try
            {
                bindCallback(Entity, keyState);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to execute bind {key} and state {keyState}.", key, keyState);
                throw;
            }
            finally
            {
                TrySetCooldown(key, keyState, DateTimeProvider.Now.AddMilliseconds(400));
            }
        }

        if (_asyncBinds.TryGetValue(key, out var asyncBindCallback))
        {
            try
            {
                await asyncBindCallback(Entity, keyState);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to execute async bind {key} and state {keyState}.", key, keyState);
                throw;
            }
            finally
            {
                TrySetCooldown(key, keyState, DateTimeProvider.Now.AddMilliseconds(400));
            }
        }
    }

    public void SetGuiDebugToolsEnabled(bool enabled)
    {
        ThrowIfDisposed();
        GuiSystemService.SetDebugToolsEnabled(_player, enabled);
    }

    public void SetText3dRenderingEnabled(bool enabled)
    {
        ThrowIfDisposed();
        Text3dService.SetRenderingEnabled(_player, enabled);
    }

    public void ToggleAllControls(bool isEnabled, bool gtaControls = true, bool mtaControls = true)
    {
        ThrowIfDisposed();
        _player.ToggleAllControls(isEnabled, gtaControls, mtaControls);
    }

    public async Task DoComplexAnimationAsync(Animation animation, bool blockMovement = true)
    {
        ThrowIfDisposed();


        var walkEnabled = _player.Controls.WalkEnabled;
        var fireEnabled = _player.Controls.FireEnabled;
        var jumpEnabled = _player.Controls.JumpEnabled;
        if (blockMovement)
        {
            _player.Controls.WalkEnabled = false;
            _player.Controls.FireEnabled = false;
            _player.Controls.JumpEnabled = false;
        }

        try
        {
            switch (animation)
            {
                case Animation.ComplexLiftUp:
                    _player.SetAnimation("freeweights", "gym_free_pickup", TimeSpan.FromSeconds(3f), false, false, false, false, null, true);
                    await Task.Delay(TimeSpan.FromSeconds(3f));
                    _player.SetAnimation("CARRY", "crry_prtial", TimeSpan.FromSeconds(0.2f), true);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        catch(Exception)
        {
            throw;
        }
        finally
        {
            if (blockMovement)
            {
                _player.Controls.WalkEnabled = walkEnabled;
                _player.Controls.FireEnabled = fireEnabled;
                _player.Controls.JumpEnabled = jumpEnabled;
            }
        }
    }

    public void StopAnimation()
    {
        _player.StopAnimation();
    }

    public void DoAnimation(Animation animation, TimeSpan? timeSpan = null)
    {
        ThrowIfDisposed();

        DoAnimationInternal(animation, ref timeSpan);
    }

    internal void DoAnimationInternal(Animation animation, ref TimeSpan? timeSpan)
    {
        switch (animation)
        {
            case Animation.StartCarry:
                timeSpan ??= TimeSpan.FromSeconds(1);
                _player.SetAnimation("CARRY", "crry_prtial", timeSpan, true, false);
                break;
            case Animation.CrouchAndTakeALook:
                timeSpan ??= TimeSpan.FromSeconds(1);
                _player.SetAnimation("COP_AMBIENT", "Copbrowse_nod", timeSpan, true, false);
                break;
            case Animation.Swing:
                timeSpan ??= TimeSpan.FromSeconds(0.5f);
                _player.SetAnimation("SWORD", "sword_block", timeSpan, false, false);
                break;
            case Animation.Click:
                timeSpan ??= TimeSpan.FromSeconds(1);
                _player.SetAnimation("CRIB", "CRIB_Use_Switch", timeSpan, true, false);
                break;
            case Animation.Eat:
                timeSpan ??= TimeSpan.FromSeconds(1);
                _player.SetAnimation("FOOD", "EAT_Burger", timeSpan, true, false);
                break;
            case Animation.Sit:
                timeSpan ??= TimeSpan.FromSeconds(1);
                _player.SetAnimation("BEACH", "ParkSit_M_loop", timeSpan, true, false);
                break;
            case Animation.CarryLiftUp:
                timeSpan ??= TimeSpan.FromSeconds(1.0f);
                _player.SetAnimation("CARRY", "liftup", timeSpan, false, false, false, false);
                break;
            case Animation.CarryPutDown:
                timeSpan ??= TimeSpan.FromSeconds(1.0f);
                _player.SetAnimation("CARRY", "putdwn", timeSpan, false, false, false, false);
                break;
            case Animation.CarryLiftUpFromTable:
                timeSpan ??= TimeSpan.FromSeconds(1.25f);
                _player.SetAnimation("CARRY", "liftup105", timeSpan, false, false, false, false);
                break;
            case Animation.CarryPutDownOnTable:
                timeSpan ??= TimeSpan.FromSeconds(1.0f);
                _player.SetAnimation("CARRY", "putdwn105", timeSpan, false, false, false, false);
                break;
            case Animation.PlantBomb:
                timeSpan ??= TimeSpan.FromSeconds(1.5f);
                _player.SetAnimation("BOMBER", "BOM_Plant", timeSpan, false, false, false, false);
                break;
            case Animation.StartFishing:
                timeSpan ??= TimeSpan.FromSeconds(1.5f);
                _player.SetAnimation("SWORD", "sword_block", timeSpan, false, false, false, true);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public async Task DoAnimationAsync(Animation animation, TimeSpan? timeSpan = null, bool blockMovement = true)
    {
        ThrowIfDisposed();

        var walkEnabled = _player.Controls.WalkEnabled;
        var fireEnabled = _player.Controls.FireEnabled;
        var jumpEnabled = _player.Controls.JumpEnabled;
        if (blockMovement)
        {
            _player.Controls.WalkEnabled = false;
            _player.Controls.FireEnabled = false;
            _player.Controls.JumpEnabled = false;
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
            if (blockMovement)
            {
                _player.Controls.WalkEnabled = walkEnabled;
                _player.Controls.FireEnabled = fireEnabled;
                _player.Controls.JumpEnabled = jumpEnabled;
            }
        }
    }

    public void Kick(PlayerDisconnectType playerDisconnectType)
    {
        ThrowIfDisposed();
        _player.Kick(playerDisconnectType);
    }

    public void Kick(string reason)
    {
        ThrowIfDisposed();
        _player.Kick(reason);
    }

    public void ShowHudComponent(HudComponent hudComponent, bool isVisible)
    {
        ThrowIfDisposed();
        _player.ShowHudComponent(hudComponent, isVisible);
    }

    public void SetFpsLimit(ushort limit)
    {
        ThrowIfDisposed();
        _player.SetFpsLimit(limit);
    }

    public void PlaySound(byte sound)
    {
        ThrowIfDisposed();
        _player.PlaySound(sound);
    }

    public void SetTransferBoxVisible(bool visible)
    {
        ThrowIfDisposed();
        _player.SetTransferBoxVisible(visible);
    }

    public override void Dispose()
    {
        _player.Kick(PlayerDisconnectType.SHUTDOWN);
        base.Dispose();
    }
}

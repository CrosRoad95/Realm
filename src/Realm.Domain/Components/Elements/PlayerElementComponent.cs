using Realm.Domain.Concepts;
using Realm.Domain.IdGenerators;
using Realm.Resources.Overlay.Interfaces;
using SlipeServer.Server.Collections;

namespace Realm.Domain.Components.Elements;

public sealed class PlayerElementComponent : ElementComponent
{
    [Inject]
    private ChatBox ChatBox { get; set; } = default!;
    [Inject]
    private OverlayService OverlayService { get; set; } = default!;
    [Inject]
    private ClientInterfaceService ClientInterfaceService { get; set; } = default!;
    [Inject]
    private AgnosticGuiSystemService AgnosticGuiSystemService { get; set; } = default!;
    [Inject]
    private Text3dService Text3dService { get; set; } = default!;
    [Inject]
    private ILogger<PlayerElementComponent> Logger { get; set; } = default!;

    private Entity? _focusedEntity;
    private readonly Player _player;
    private readonly Vector2 _screenSize;
    private readonly Dictionary<string, Func<Entity, KeyState, Task>> _binds = new();
    private readonly SemaphoreSlim _bindsLock = new(1);
    private readonly SemaphoreSlim _bindsUpLock = new(1);
    private readonly SemaphoreSlim _bindsDownLock = new(1);
    private readonly object _bindsCooldownLock = new();

    private readonly Dictionary<string, DateTime> _bindsDownCooldown = new();
    private readonly Dictionary<string, DateTime> _bindsUpCooldown = new();
    private readonly HashSet<string> _enableFightFlags = new();
    private readonly MapIdGenerator _mapIdGenerator = new(IdGeneratorConstants.MapIdStart, IdGeneratorConstants.MapIdStop);
    private readonly Dictionary<string, IDisposable> _huds = new();
    private readonly object _hudsLock = new();
    public event Action<Entity, Entity?>? FocusedEntityChanged;
    public Entity? FocusedEntity { get => _focusedEntity; internal set
        {
            ThrowIfDisposed();
            if (value != _focusedEntity)
            {
                _focusedEntity = value;
                FocusedEntityChanged?.Invoke(Entity, value);
            }
        }
    }
    public Vector2 ScreenSize => _screenSize;

    internal Player Player => _player;
    internal bool Spawned { get; set; }
    public string Name { get => Player.Name; set => Player.Name = value; }
    public string Language { get; private set; } = "pl";
    public WeaponCollection Weapons => _player.Weapons;

    internal override Element Element => _player;
    internal MapIdGenerator MapIdGenerator => _mapIdGenerator;

    public Entity? OccupiedVehicle
    {
        get
        {
            ThrowIfDisposed();
            if (_player.Vehicle == null)
                return null;
            return EntityByElement.TryGetByElement(_player.Vehicle);
        }
    }

    internal PlayerElementComponent(Player player, Vector2 screenSize)
    {
        _player = player;
        _screenSize = screenSize;
    }

    protected override void Load()
    {
        ThrowIfDisposed();
        Entity.Transform.Bind(_player);
        _player.BindExecuted += HandleBindExecuted;
        UpdateFight();
    }

    private void UpdateFight()
    {
        ThrowIfDisposed();
        _player.Controls.FireEnabled = _enableFightFlags.Any();
    }

    public bool AddEnableFightFlag(string flag)
    {
        ThrowIfDisposed();
        if (_enableFightFlags.Add(flag))
        {
            UpdateFight();
            return true;
        }
        return false;
    }
    
    public bool RemoveEnableFightFlag(string flag)
    {
        ThrowIfDisposed();
        if (_enableFightFlags.Remove(flag))
        {
            UpdateFight();
            return true;
        }
        return false;
    }

    public bool Compare(Player player) => _player == player;

    public bool TrySpawnAtLastPosition()
    {
        ThrowIfDisposed();
        var accountComponent = Entity.GetComponent<AccountComponent>();
        if (accountComponent != null)
        {
            var lastTransformAndMotion = accountComponent.User.LastTransformAndMotion;
            if (lastTransformAndMotion != null)
            {
                Spawn(lastTransformAndMotion.Position, lastTransformAndMotion.Rotation);
                return true;
            }
        }

        return false;
    }

    public void Spawn(Vector3 position, Vector3? rotation = null)
    {
        ThrowIfDisposed();
        _player.Camera.Target = _player;
        _player.Spawn(position, rotation?.Z ?? 0, 0, 0, 0);
        _player.Rotation = rotation ?? Vector3.Zero;
        Entity.Transform.Position = position;
        Spawned = true;
    }

    public void SendChatMessage(string message, Color? color = null, bool isColorCoded = false)
    {
        ThrowIfDisposed();
        ChatBox.OutputTo(_player, message, color ?? Color.White, isColorCoded);
    }

    public void ClearChatBox()
    {
        ThrowIfDisposed();
        ChatBox.ClearFor(_player);
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

    public void SetChatVisible(bool visible)
    {
        ThrowIfDisposed();
        ChatBox.SetVisibleFor(_player, visible);
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

    public void SetBind(string key, Func<Entity, KeyState, Task> callback)
    {
        ThrowIfDisposed();

        _bindsLock.Wait();
        if (_binds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindAlreadyExistsException(key);
        }

        _player.SetBind(key, KeyState.Both);
        _binds[key] = callback;
        _bindsLock.Release();

    }
    
    public void SetBind(string key, Func<Entity, Task> callback)
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
            if(keyState == KeyState.Down)
                 return callback(entity);
            return Task.CompletedTask;
        };
        _bindsLock.Release();
    }

    public void Unbind(string key)
    {
        ThrowIfDisposed();
        _bindsLock.Wait();
        if (!_binds.ContainsKey(key))
        {
            _bindsLock.Release();
            throw new BindDoesntExistsException(key);
        }
        _player.RemoveBind(key, KeyState.Both);
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

        if (cooldownUntil > DateTime.Now)
        {
            return true;
        }
        return false;
    }

    public bool IsCooldownActive(string key, KeyState keyState = KeyState.Down)
    {
        if(keyState == KeyState.Down)
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

        if(IsCooldownActive(key, keyState))
            return;

        // Lock bind indefinitly in case of bind takes a long time to execute, reset cooldown to unlock
        SetCooldown(key, keyState, DateTime.MaxValue);

        try
        {
            await _binds[key](Entity, keyState);
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Failed to execute bind {key} and state {keyState}.", key, keyState);
            throw;
        }
        finally
        {
            TrySetCooldown(key, keyState, DateTime.Now.AddMilliseconds(400));
        }
    }

    public void SetGuiDebugToolsEnabled(bool enabled)
    {
        ThrowIfDisposed();
        AgnosticGuiSystemService.SetDebugToolsEnabled(_player, enabled);
    }

    public void SetText3dRenderingEnabled(bool enabled)
    {
        ThrowIfDisposed();
        Text3dService.SetRenderingEnabled(_player, enabled);
    }

    public void SasdetText3dRenderingEnabled(bool enabled)
    {
        ThrowIfDisposed();
        Text3dService.SetRenderingEnabled(_player, enabled);
    }

    public enum Animation
    {
        StartCarry,
        CrouchAndTakeALook,
        Swing,
        Click,
        Eat,
        Sit,
        CarryLiftUp,
        CarryPutDown,
        CarryLiftUpFromTable,
        CarryPutDownOnTable,

        // Complex animations
        ComplexLiftUp,
    }

    public async Task DoComplexAnimationAsync(Animation animation, bool blockMovement = true)
    {
        ThrowIfDisposed();

        if (blockMovement)
            _player.ToggleAllControls(false, true, false);

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

        if (blockMovement)
            _player.ToggleAllControls(true, true, false);
    }

    public void DoAnimation(Animation animation, TimeSpan? timeSpan = null)
    {
        ThrowIfDisposed();

        DoAnimationInternal(animation, ref timeSpan);
    }

    public void DoAnimationInternal(Animation animation, ref TimeSpan? timeSpan)
    {
        switch(animation)
        {
            case Animation.StartCarry:
                timeSpan = timeSpan ?? TimeSpan.FromSeconds(1);
                _player.SetAnimation("CARRY", "crry_prtial", timeSpan, true, false);
                break;
            case Animation.CrouchAndTakeALook:
                timeSpan = timeSpan ?? TimeSpan.FromSeconds(1);
                _player.SetAnimation("COP_AMBIENT", "Copbrowse_nod", timeSpan, true, false);
                break;
            case Animation.Swing:
                timeSpan = timeSpan ?? TimeSpan.FromSeconds(0.5f);
                _player.SetAnimation("SWORD", "sword_block", timeSpan, false, false);
                break;
            case Animation.Click:
                timeSpan = timeSpan ?? TimeSpan.FromSeconds(1);
                _player.SetAnimation("CRIB", "CRIB_Use_Switch", timeSpan, true, false);
                break;
            case Animation.Eat:
                timeSpan = timeSpan ?? TimeSpan.FromSeconds(1);
                _player.SetAnimation("FOOD", "EAT_Burger", timeSpan, true, false);
                break;
            case Animation.Sit:
                timeSpan = timeSpan ?? TimeSpan.FromSeconds(1);
                _player.SetAnimation("BEACH", "ParkSit_M_loop", timeSpan, true, false);
                break;
            case Animation.CarryLiftUp:
                timeSpan = timeSpan ?? TimeSpan.FromSeconds(1.25f);
                _player.SetAnimation("CARRY", "liftup", timeSpan, false, false, false, false);
                break;
            case Animation.CarryPutDown:
                timeSpan = timeSpan ?? TimeSpan.FromSeconds(1.0f);
                _player.SetAnimation("CARRY", "putdwn", timeSpan, false, false, false, false);
                break;
            case Animation.CarryLiftUpFromTable:
                timeSpan = timeSpan ?? TimeSpan.FromSeconds(1.25f);
                _player.SetAnimation("CARRY", "liftup105", timeSpan, false, false, false, false);
                break;
            case Animation.CarryPutDownOnTable:
                timeSpan = timeSpan ?? TimeSpan.FromSeconds(1.0f);
                _player.SetAnimation("CARRY", "putdwn105", timeSpan, false, false, false, false);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public async Task DoAnimationAsync(Animation animation, TimeSpan? timeSpan = null, bool blockMovement = true)
    {
        ThrowIfDisposed();

        if (blockMovement)
            _player.ToggleAllControls(false, true, false);

        try
        {
            DoAnimationInternal(animation, ref timeSpan);
            if(timeSpan != null)
                await Task.Delay(timeSpan.Value);
        }
        catch(Exception)
        {
            throw;
        }
        finally
        {
            if (blockMovement)
                _player.ToggleAllControls(true, true, false);
        }
    }

    public IHud<object> CreateHud(string hudId, Action<IHudBuilder<object>> hudBuilderCallback, Vector2? offset = null)
    {
        lock (_hudsLock)
        {
            if(_huds.ContainsKey(hudId))
                throw new Exception("Hud name already in use");

            OverlayService.CreateHud(_player, hudId, hudBuilderCallback, _screenSize, offset);
            var hudController = new Hud<object>(hudId, _player, OverlayService, offset, null);
            _huds[hudId] = hudController;
            return hudController;
        }
    }

    public void RemoveHud(string hudId)
    {
        lock(_hudsLock)
        {
            if(!_huds.ContainsKey(hudId))
                throw new Exception("Hud with this does not exists.");

            _huds.Remove(hudId);
        }
        OverlayService.RemoveHud(_player, hudId);
    }

    public override void Dispose()
    {
        lock(_hudsLock)
        {
            foreach (var item in _huds)
            {
                item.Value.Dispose();
            }
            _huds.Clear();
        }
        base.Dispose();
        _player.Kick(PlayerDisconnectType.SHUTDOWN);
    }
}

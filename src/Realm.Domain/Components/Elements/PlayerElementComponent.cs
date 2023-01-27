using Realm.Domain.IdGenerators;

namespace Realm.Domain.Components.Elements;

public sealed class PlayerElementComponent : ElementComponent
{
    [Inject]
    private ChatBox ChatBox { get; set; } = default!;
    [Inject]
    private OverlayNotificationsService OverlayNotificationsService { get; set; } = default!;
    [Inject]
    private ClientInterfaceService ClientInterfaceService { get; set; } = default!;
    [Inject]
    private AgnosticGuiSystemService AgnosticGuiSystemService { get; set; } = default!;
    [Inject]
    private Text3dService Text3dService { get; set; } = default!;

    private Entity? _focusedEntity;
    private readonly Player _player;
    private readonly Dictionary<string, Func<Entity, Task>> _binds = new();
    private readonly Dictionary<string, DateTime> _bindsCooldown = new();
    private readonly HashSet<string> _enableFightFlags = new();
    private readonly MapIdGenerator _mapIdGenerator = new(IdGeneratorConstants.MapIdStart, IdGeneratorConstants.MapIdStop);

    public event Action<Entity, Entity?>? FocusedEntityChanged;
    public Entity? FocusedEntity { get => _focusedEntity; internal set
        {
            if(value != _focusedEntity)
            {
                _focusedEntity = value;
                FocusedEntityChanged?.Invoke(Entity, value);
            }
        }
    }

    internal Player Player => _player;
    public string Name { get => Player.Name; set => Player.Name = value; }
    public string Language { get; private set; } = "pl";

    internal override Element Element => _player;
    internal MapIdGenerator MapIdGenerator => _mapIdGenerator;

    public Entity? OccupiedVehicle
    {
        get
        {
            if (_player.Vehicle == null)
                return null;
            return EntityByElement.TryGetByElement(_player.Vehicle);
        }
    }

    internal PlayerElementComponent(Player player)
    {
        _player = player;
    }

    public override void Load()
    {
        Entity.Transform.Bind(_player);
        _player.BindExecuted += HandleBindExecuted;
        _player.Controls.FireEnabled = false;
    }

    public bool AddEnableFightFlag(string flag)
    {
        if(_enableFightFlags.Add(flag))
        {
            _player.Controls.FireEnabled = _enableFightFlags.Any();
            return true;
        }
        return false;
    }
    
    public bool RemoveEnableFightFlag(string flag)
    {
        if(_enableFightFlags.Remove(flag))
        {
            _player.Controls.FireEnabled = _enableFightFlags.Any();
            return true;
        }
        return false;
    }

    public override void Dispose()
    {
        base.Dispose();
        _player.Kick(PlayerDisconnectType.SHUTDOWN);
    }

    public bool Compare(Player player) => _player == player;

    public bool TrySpawnAtLastPosition()
    {
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
        _player.Camera.Target = _player;
        _player.Spawn(position, rotation?.Z ?? 0, 0, 0, 0);
        _player.Rotation = rotation ?? Vector3.Zero;
        if(position != Vector3.Zero)
        {
            Task.Run(async () =>
            {
                await Task.Delay(250);
                if(Entity.Transform.Position.LengthSquared() < 7)
                {
                    Entity.Transform.Position = position;
                }
            });
        }
    }

    public void SendChatMessage(string message, Color? color = null, bool isColorCoded = false)
    {
        ChatBox.OutputTo(_player, message, color ?? Color.White, isColorCoded);
    }

    public void ClearChatBox()
    {
        ChatBox.ClearFor(_player);
    }
    
    public void SetCameraMatrix(Vector3 from, Vector3 to)
    {
        _player.Camera.SetMatrix(from, to);
    }

    public void FadeCamera(CameraFade cameraFade, float fadeTime = 0.5f)
    {
        _player.Camera.Fade(cameraFade, fadeTime);
    }

    public Task FadeCameraAsync(CameraFade cameraFade, float fadeTime = 0.5f)
    {
        _player.Camera.Fade(cameraFade, fadeTime);
        return Task.Delay(TimeSpan.FromSeconds(fadeTime));
    }

    public void SetChatVisible(bool visible)
    {
        ChatBox.SetVisibleFor(_player, visible);
    }
    
    public void SetCameraTarget(Entity entity)
    {
        var elementComponent = entity.GetRequiredComponent<ElementComponent>();
        _player.Camera.Target = elementComponent.Element;
    }

    #region ClientInterface resource
    public void SetClipboard(string content)
    {
        ClientInterfaceService.SetClipboard(_player, content);
    }
    #endregion

    #region Overlay resource
    public void AddNotification(string message)
    {
        OverlayNotificationsService.AddNotification(_player, message);
    }
    #endregion

    public void SetBind(string key, Func<Entity, Task> callback)
    {
        if (_binds.ContainsKey(key))
            throw new BindAlreadyExistsException(key);

        _player.SetBind(key, KeyState.Up);
        _binds[key] = callback;
    }

    public void ResetCooldown(string key)
    {
        _bindsCooldown.Remove(key);
    }

    private async void HandleBindExecuted(Player sender, PlayerBindExecutedEventArgs e)
    {
        if (!_binds.ContainsKey(e.Key))
            return;

        if(_bindsCooldown.TryGetValue(e.Key, out var cooldownUntil))
        {
            if (cooldownUntil > DateTime.Now)
                return;
        }
        _bindsCooldown[e.Key] = DateTime.MaxValue; // Lock bind indefinitly in case of bind takes a long time to execute
        await _binds[e.Key](Entity);
        if(_bindsCooldown.ContainsKey(e.Key)) // Wasn't bind cooldown reset?
            _bindsCooldown[e.Key] = DateTime.Now.AddMilliseconds(400);
    }

    public void SetGuiDebugToolsEnabled(bool enabled)
    {
        AgnosticGuiSystemService.SetDebugToolsEnabled(_player, enabled);
    }

    public void SetRenderingEnabled(bool enabled)
    {
        Text3dService.SetRenderingEnabled(_player, enabled);
    }

    public async Task DoAnimation()
    {

    }
}

namespace Realm.Domain.Components.Elements;

public sealed class PlayerElementComponent : ElementComponent
{
    [Inject]
    private ChatBox ChatBox { get; set; } = default!;
    [Inject]
    private OverlayNotificationsService OverlayNotificationsService { get; set; } = default!;
    [Inject]
    private LuaInteropService LuaInteropService { get; set; } = default!;
    [Inject]
    private LuaValueMapper LuaValueMapper { get; set; } = default!;
    [Inject]
    private LuaEventService LuaEventService { get; set; } = default!;
    [Inject]
    private AgnosticGuiSystemService AgnosticGuiSystemService { get; set; } = default!;

    private readonly Player _player;
    private readonly Dictionary<string, Func<Entity, Task>> _binds = new();
    private readonly Dictionary<string, DateTime> _bindsCooldown = new();

    public Player Player => _player;

    public string Name { get => Player.Name; set => Player.Name = value; }

    public string Language { get; private set; } = "pl";

    public override Element Element => _player;

    public Entity? OccupiedVehicle
    {
        get
        {
            if (_player.Vehicle == null)
                return null;
            return EntityByElement.TryGetByElement(_player.Vehicle);
        }
    }

    public PlayerElementComponent(Player player)
    {
        _player = player;
    }

    public override Task Load()
    {
        Entity.Transform.Bind(_player);
        _player.BindExecuted += HandleBindExecuted;
        return Task.CompletedTask;
    }

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
        _player.Camera.Fade(CameraFade.In);
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

    public void SetChatVisible(bool visible)
    {
        ChatBox.SetVisibleFor(_player, visible);
    }

    #region LuaInterop resource
    public void SetClipboard(string content)
    {
        LuaInteropService.SetClipboard(_player, content);
    }
    #endregion

    #region Overlay resource
    public void AddNotification(string message)
    {
        OverlayNotificationsService.AddNotification(_player, message);
    }
    #endregion

    // TODO: improve
    public void TriggerClientEvent(string name, object[] values)
    {
        LuaValue luaValue;
        if (values.Length == 1 && values[0].GetType() == typeof(object[]))
        {
            luaValue = ((object[])values[0]).Select(LuaValueMapper.Map).ToArray();
        }
        else
        {
            luaValue = values.Select(LuaValueMapper.Map).ToArray();
        }

        LuaEventService.TriggerEventFor(_player, name, _player, luaValue);
    }

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
}

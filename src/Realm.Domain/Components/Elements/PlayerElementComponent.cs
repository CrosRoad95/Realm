using Realm.Resources.LuaInterop;
using Realm.Resources.Overlay;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace Realm.Domain.Components.Elements;

public sealed class PlayerElementComponent : Component
{
    private readonly Player _player;

    public Player Player => _player;

    public string Name { get => Player.Name; set => Player.Name = value; }

    public string Language { get; private set; } = "pl";

    public PlayerElementComponent(Player player)
    {
        _player = player;
    }

    public override Task Load()
    {
        Entity.Transform.Bind(_player);
        return Task.CompletedTask;
    }

    public void Spawn(Vector3 position, Vector3? rotation = null)
    {
        _player.Camera.Target = _player;
        _player.Camera.Fade(CameraFade.In);
        _player.Spawn(position, rotation?.Z ?? 0, 0, 0, 0);
    }

    public void SendChatMessage(string message, Color? color = null, bool isColorCoded = false)
    {
        var chatbox = Entity.GetRequiredService<ChatBox>();
        chatbox.OutputTo(_player, message, color ?? Color.White, isColorCoded);
    }

    public void ClearChatBox()
    {
        var chatbox = Entity.GetRequiredService<ChatBox>();
        chatbox.ClearFor(_player);
    }

    #region LuaInterop resource
    public void SetClipboard(string content)
    {
        var luaInteropService = Entity.GetRequiredService<LuaInteropService>();
        luaInteropService.SetClipboard(_player, content);
    }
    #endregion

    #region Overlay resource
    public void AddNotification(string message)
    {
        var overlayNotificationsService = Entity.GetRequiredService<OverlayNotificationsService>();
        overlayNotificationsService.AddNotification(_player, message);
    }
    #endregion

    // TODO: improve
    public void TriggerClientEvent(string name, object[] values)
    {
        var luaValueMapper = Entity.GetRequiredService<LuaValueMapper>();
        var luaEventService = Entity.GetRequiredService<LuaEventService>();
        LuaValue luaValue;
        if (values.Length == 1 && values[0].GetType() == typeof(object[]))
        {
            luaValue = ((object[])values[0]).Select(luaValueMapper.Map).ToArray();
        }
        else
        {
            luaValue = values.Select(luaValueMapper.Map).ToArray();
        }

        luaEventService.TriggerEventFor(_player, name, _player, luaValue);
    }
}

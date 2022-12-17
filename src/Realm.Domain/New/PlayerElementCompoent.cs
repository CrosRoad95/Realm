using Realm.Common.Utilities;
using Realm.Resources.AdminTools;
using Realm.Resources.LuaInterop;
using Realm.Resources.Overlay;
using Serilog.Context;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Lua.Camera;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;
using System.Drawing;

namespace Realm.Domain.New;

[NoDefaultScriptAccess]
public sealed class PlayerElementCompoent : Component
{
    private readonly Player _player;

    public Player Player => _player;

    private const int _RESOURCE_COUNT = 8;

    [ScriptMember("name")]
    public string Name { get => Player.Name; set => Player.Name = value; }

    [ScriptMember("language", ScriptAccess.ReadOnly)]
    public string Language { get; private set; } = "pl";

    public PlayerElementCompoent(Player player)
    {
        _player = player;
    }

    public override Task Load()
    {
        Entity.Transform.Bind(_player);
        return Task.CompletedTask;
    }

    [ScriptMember("spawn")]
    public void Spawn(Vector3 position, Vector3? rotation = null)
    {
        _player.Camera.Target = _player;
        _player.Camera.Fade(CameraFade.In);
        _player.Spawn(position, rotation?.Z ?? 0, 0, 0, 0);
    }

    [ScriptMember("sendChatMessage")]
    public void SendChatMessage(string message, Color? color = null, bool isColorCoded = false)
    {
        var chatbox = Entity.GetRequiredService<ChatBox>();
        chatbox.OutputTo(_player, message, color ?? Color.White, isColorCoded);
    }

    [ScriptMember("clearChatBox")]
    public void ClearChatBox()
    {
        var chatbox = Entity.GetRequiredService<ChatBox>();
        chatbox.ClearFor(_player);
    }

    #region LuaInterop resource
    [ScriptMember("setClipboard")]
    public void SetClipboard(string content)
    {
        var luaInteropService = Entity.GetRequiredService<LuaInteropService>();
        luaInteropService.SetClipboard(_player, content);
    }
    #endregion

    #region Overlay resource
    [ScriptMember("addNotification")]
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

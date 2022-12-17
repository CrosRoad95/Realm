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
    private readonly Player _rpgPlayer;

    public Player Player => _rpgPlayer;

    private const int _RESOURCE_COUNT = 8;

    [ScriptMember("name")]
    public string Name { get => Player.Name; set => Player.Name = value; }

    [ScriptMember("language", ScriptAccess.ReadOnly)]
    public string Language { get; private set; } = "pl";

    public PlayerElementCompoent(Player player)
    {
        _rpgPlayer = player;
    }

    [ScriptMember("spawn")]
    public void Spawn(Vector3 position, Vector3? rotation = null)
    {
        var player = Entity.InternalGetRequiredComponent<PlayerElementCompoent>().Player;

        player.Camera.Target = player;
        player.Camera.Fade(CameraFade.In);
        player.Spawn(position, rotation?.Z ?? 0, 0, 0, 0);
    }

    [ScriptMember("sendChatMessage")]
    public void SendChatMessage(string message, Color? color = null, bool isColorCoded = false)
    {
        var player = Entity.InternalGetRequiredComponent<PlayerElementCompoent>().Player;
        var chatbox = Entity.GetRequiredService<ChatBox>();
        chatbox.OutputTo(player, message, color ?? Color.White, isColorCoded);
    }

    [ScriptMember("clearChatBox")]
    public void ClearChatBox()
    {
        var player = Entity.InternalGetRequiredComponent<PlayerElementCompoent>().Player;
        var chatbox = Entity.GetRequiredService<ChatBox>();
        chatbox.ClearFor(player);
    }

    #region LuaInterop resource
    [ScriptMember("setClipboard")]
    public void SetClipboard(string content)
    {
        var player = Entity.InternalGetRequiredComponent<PlayerElementCompoent>().Player;
        var luaInteropService = Entity.GetRequiredService<LuaInteropService>();
        luaInteropService.SetClipboard(player, content);
    }
    #endregion

    #region Overlay resource
    [ScriptMember("addNotification")]
    public void AddNotification(string message)
    {
        var player = Entity.InternalGetRequiredComponent<PlayerElementCompoent>().Player;
        var overlayNotificationsService = Entity.GetRequiredService<OverlayNotificationsService>();
        overlayNotificationsService.AddNotification(player,message);
    }
    #endregion

    // TODO: improve
    public void TriggerClientEvent(string name, object[] values)
    {
        var luaValueMapper = Entity.GetRequiredService<LuaValueMapper>();
        var luaEventService = Entity.GetRequiredService<LuaEventService>();
        var player = Entity.InternalGetRequiredComponent<PlayerElementCompoent>().Player;
        LuaValue luaValue;
        if (values.Length == 1 && values[0].GetType() == typeof(object[]))
        {
            luaValue = ((object[])values[0]).Select(luaValueMapper.Map).ToArray();
        }
        else
        {
            luaValue = values.Select(luaValueMapper.Map).ToArray();
        }

        luaEventService.TriggerEventFor(player, name, player, luaValue);
    }
}

using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using SlipeServer.Server;
using System.Numerics;
using SlipeServer.Packets.Definitions.Lua;

namespace Realm.Resources.Overlay;

internal class OverlayLogic
{
    private readonly LuaEventService _luaEventService;
    private readonly OverlayService _overlayService;
    private readonly OverlayResource _resource;

    public OverlayLogic(MtaServer server, LuaEventService luaEventService,
        OverlayService overlayService)
    {
        _luaEventService = luaEventService;
        _overlayService = overlayService;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<OverlayResource>();

        _overlayService.NotificationAdded += HandleNotificationAdded;
        _overlayService.HudCreated = HandleHudCreated;
        _overlayService.HudRemoved = HandleHudRemoved;
        _overlayService.HudVisibilityChanged = HandleHudVisibilityChanged;
        _overlayService.HudPositionChanged = HandleHudPositionChanged;
        _overlayService.HudStateChanged = HandleHudStateChanged;
    }

    private void HandleNotificationAdded(Player player, string message)
    {
        _luaEventService.TriggerEventFor(player, "addNotification", player, message);
    }
    
    public void HandleHudVisibilityChanged(Player player, string hudId, bool visible)
    {
        _luaEventService.TriggerEventFor(player, "setHudVisible", player, hudId, visible);
    }

    public void HandleHudPositionChanged(Player player, string hudId, float x, float y)
    {
        _luaEventService.TriggerEventFor(player, "setHudPosition", player, hudId, x, y);
    }

    public void HandleHudStateChanged(Player player, string hudId, Dictionary<int, object> keyValuePairs)
    {
        Dictionary<LuaValue, LuaValue> luaValue = new();
        foreach (var item in keyValuePairs)
        {
            var stringValue = item.Value.ToString();
            if(stringValue != null)
            {
                if (double.TryParse(stringValue, out var result))
                    luaValue.Add(item.Key, result);
                else
                    luaValue.Add(item.Key, stringValue);
            }
        }
        _luaEventService.TriggerEventFor(player, "setHudState", player, hudId, new LuaValue(luaValue));
    }

    private void HandleHudCreated(Player player, string hudId, Vector2 position, IEnumerable<LuaValue> hudElementsDefinitions)
    {
        _luaEventService.TriggerEventFor(player, "createHud", player, hudId, position.X, position.Y, hudElementsDefinitions);
    }
    
    private void HandleHudRemoved(Player player, string hudId)
    {
        _luaEventService.TriggerEventFor(player, "removeHud", player, hudId);
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}

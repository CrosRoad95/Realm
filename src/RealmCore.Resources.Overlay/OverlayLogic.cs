using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using SlipeServer.Server;
using System.Numerics;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Mappers;

namespace RealmCore.Resources.Overlay;

internal class OverlayLogic
{
    private readonly LuaEventService _luaEventService;
    private readonly IOverlayService _overlayService;
    private readonly RootElement _rootElement;
    private readonly LuaValueMapper _luaValueMapper;
    private readonly OverlayResource _resource;

    public OverlayLogic(MtaServer server, LuaEventService luaEventService,
        IOverlayService overlayService, RootElement rootElement, LuaValueMapper luaValueMapper)
    {
        _luaEventService = luaEventService;
        _overlayService = overlayService;
        _rootElement = rootElement;
        _luaValueMapper = luaValueMapper;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<OverlayResource>();

        _overlayService.NotificationAdded += HandleNotificationAdded;
        _overlayService.HudCreated = HandleHudCreated;
        _overlayService.Hud3dCreated = HandleHud3dCreated;
        _overlayService.HudRemoved = HandleHudRemoved;
        _overlayService.Hud3dRemoved = HandleHud3dRemoved;
        _overlayService.HudVisibilityChanged = HandleHudVisibilityChanged;
        _overlayService.HudPositionChanged = HandleHudPositionChanged;
        _overlayService.HudStateChanged = HandleHudStateChanged;
        _overlayService.Hud3dStateChanged = HandleHud3dStateChanged;
        _overlayService.Display3dRingAdded = HandleDisplay3dRingAdded;
        _overlayService.Display3dRingRemoved = HandleDisplay3dRingRemoved;
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

    public void HandleHud3dStateChanged(string hudId, Dictionary<int, object> keyValuePairs)
    {
        Dictionary<LuaValue, LuaValue> luaValue = new();
        foreach (var item in keyValuePairs)
        {
            luaValue.Add(item.Key, _luaValueMapper.Map(item.Value));
        }
        _luaEventService.TriggerEvent("setHud3dState", _rootElement, hudId, new LuaValue(luaValue));
    }

    public void HandleHudStateChanged(Player player, string hudId, Dictionary<int, object> keyValuePairs)
    {
        Dictionary<LuaValue, LuaValue> luaValue = new();
        foreach (var item in keyValuePairs)
        {
            luaValue.Add(item.Key, _luaValueMapper.Map(item.Value));
        }
        _luaEventService.TriggerEventFor(player, "setHudState", player, hudId, new LuaValue(luaValue));
    }

    private void HandleHudCreated(Player player, string hudId, Vector2 position, IEnumerable<LuaValue> hudElementsDefinitions)
    {
        _luaEventService.TriggerEventFor(player, "createHud", player, hudId, position.X, position.Y, hudElementsDefinitions);
    }

    private void HandleHud3dCreated(string hudId, Vector3 position, IEnumerable<LuaValue> hudElementsDefinitions)
    {
        _luaEventService.TriggerEvent("createHud3d", _rootElement, hudId, hudElementsDefinitions, position.X, position.Y, position.Z);
    }

    private void HandleHudRemoved(Player player, string hudId)
    {
        _luaEventService.TriggerEventFor(player, "removeHud", player, hudId);
    }

    private void HandleHud3dRemoved(string hudId)
    {
        _luaEventService.TriggerEvent("removeHud3d", _rootElement, hudId);
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void HandleDisplay3dRingAdded(Player player, string id, Vector3 position, TimeSpan time)
    {
        _luaEventService.TriggerEvent("addDisplay3dRing", player, id, position.X, position.Y, position.Z, time.TotalMilliseconds);
    }

    private void HandleDisplay3dRingRemoved(Player player, string id)
    {
        _luaEventService.TriggerEvent("removeDisplay3dRing", player, id);
    }
}

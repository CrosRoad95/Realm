using SlipeServer.Server.Elements;
using SlipeServer.Server;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Mappers;
using RealmCore.Resources.Base.Interfaces;

namespace RealmCore.Resources.Overlay;

internal class OverlayLogic
{
    private readonly IOverlayService _overlayService;
    private readonly RootElement _rootElement;
    private readonly LuaValueMapper _luaValueMapper;
    private readonly ILuaEventHub<IHudEventHub> _luaEventHub;
    private readonly OverlayResource _resource;

    public OverlayLogic(MtaServer server, IOverlayService overlayService, RootElement rootElement, LuaValueMapper luaValueMapper, ILuaEventHub<IHudEventHub> luaEventHub)
    {
        _overlayService = overlayService;
        _rootElement = rootElement;
        _luaValueMapper = luaValueMapper;
        _luaEventHub = luaEventHub;
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
        _luaEventHub.Broadcast(x => x.AddNotification(message), player);
    }

    public void HandleHudVisibilityChanged(Player player, string hudId, bool visible)
    {
        _luaEventHub.Invoke(player, x => x.SetHudVisible(hudId, visible), player);
    }

    public void HandleHudPositionChanged(Player player, string hudId, float px, float py)
    {
        _luaEventHub.Invoke(player, x => x.SetHudPosition(hudId, px, py), player);
    }

    public void HandleHud3dStateChanged(string hudId, Dictionary<int, object?> keyValuePairs)
    {
        Dictionary<LuaValue, LuaValue> luaValue = [];
        foreach (var item in keyValuePairs)
        {
            luaValue.Add(item.Key, _luaValueMapper.Map(item.Value));
        }
        _luaEventHub.Broadcast(x => x.SetHud3dState(hudId, new LuaValue(luaValue)));
    }

    public void HandleHudStateChanged(Player player, string hudId, Dictionary<int, object?> keyValuePairs)
    {
        Dictionary<LuaValue, LuaValue> luaValue = [];
        foreach (var item in keyValuePairs)
        {
            luaValue.Add(item.Key, _luaValueMapper.Map(item.Value));
        }
        _luaEventHub.Invoke(player, x => x.SetHudState(hudId, new LuaValue(luaValue)), player);
    }

    private void HandleHudCreated(Player player, string hudId, Vector2 position, IEnumerable<LuaValue> hudElementsDefinitions)
    {
        _luaEventHub.Invoke(player, x => x.CreateHud(hudId, position.X, position.Y, hudElementsDefinitions), player);
    }

    private void HandleHud3dCreated(string hudId, Vector3 position, IEnumerable<LuaValue> hudElementsDefinitions)
    {
        _luaEventHub.Broadcast(x => x.CreateHud3d(hudId, position.X, position.Y, position.Z, hudElementsDefinitions));
    }

    private void HandleHudRemoved(Player player, string hudId)
    {
        _luaEventHub.Invoke(player, x => x.RemoveHud(hudId), player);
    }

    private void HandleHud3dRemoved(string hudId)
    {
        _luaEventHub.Broadcast(x => x.RemoveHud3d(hudId));
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void HandleDisplay3dRingAdded(Player player, string id, Vector3 position, TimeSpan time)
    {
        _luaEventHub.Broadcast(x => x.AddDisplay3dRing(id, position.X, position.Y, position.Z, time.TotalMilliseconds));
    }

    private void HandleDisplay3dRingRemoved(Player player, string id)
    {
        _luaEventHub.Broadcast(x => x.RemoveDisplay3dRing(id));
    }
}

using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;
using SlipeServer.Server;
using System.Numerics;
using System.Drawing;
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
    }

    private void HandleNotificationAdded(Player player, string message)
    {
        _luaEventService.TriggerEventFor(player, "addNotification", player, message);
    }
    
    public void HandleHudVisibilityChanged(Player player, string hudId, bool visible)
    {
        _luaEventService.TriggerEventFor(player, "setHudVisible", player, hudId, visible);
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

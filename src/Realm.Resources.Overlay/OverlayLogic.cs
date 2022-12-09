using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;
using SlipeServer.Server;
using Realm.Resources.Overlay.Events;

namespace Realm.Resources.Overlay;

internal class OverlayLogic
{
    private readonly LuaEventService _luaEventService;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly OverlayNotificationsService _overlayService;
    private readonly OverlayResource _resource;

    public OverlayLogic(MtaServer server, LuaEventService luaEventService, FromLuaValueMapper fromLuaValueMapper,
        OverlayNotificationsService overlayService)
    {
        _luaEventService = luaEventService;
        _fromLuaValueMapper = fromLuaValueMapper;
        _overlayService = overlayService;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<OverlayResource>();

        _overlayService.NotificationAdded += HandleNotificationAdded;
    }

    private void HandleNotificationAdded(AddNotificationEvent addNotification)
    {
        _luaEventService.TriggerEventFor(addNotification.Player, "addNotification", addNotification.Player, addNotification.Message);
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}

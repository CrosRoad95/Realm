using Realm.Domain.Elements;
using Realm.Resources.Overlay;

namespace Realm.Server.Logic.Resources;

internal class OverlayLogic
{
    private readonly MtaServer _mtaServer;
    private readonly OverlayNotificationsService _overlayNotificationsService;

    public OverlayLogic(MtaServer mtaServer, OverlayNotificationsService overlayNotificationsService)
    {
        _mtaServer = mtaServer;
        _overlayNotificationsService = overlayNotificationsService;
        _mtaServer.PlayerJoined += MtaServer_PlayerJoined;
    }

    private void MtaServer_PlayerJoined(Player player)
    {
        RPGPlayer rpgPlayer = (RPGPlayer)player;
        rpgPlayer.NotificationAdded += rpgPlayer_NotificationAdded;
    }

    private void rpgPlayer_NotificationAdded(RPGPlayer player, string message)
    {
        _overlayNotificationsService.AddNotification(player, message);
    }
}

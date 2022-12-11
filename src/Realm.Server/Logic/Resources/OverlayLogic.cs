namespace Realm.Server.Logic.Resources;

internal class OverlayLogic
{
    private readonly MtaServer _mtaServer;
    private readonly OverlayNotificationsService _overlayNotificationsService;

    public OverlayLogic(MtaServer mtaServer, OverlayNotificationsService overlayNotificationsService)
    {
        _mtaServer = mtaServer;
        _overlayNotificationsService = overlayNotificationsService;
        _mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        RPGPlayer rpgPlayer = (RPGPlayer)player;
        rpgPlayer.NotificationAdded += HandleNotificationAdded;
    }

    private void HandleNotificationAdded(RPGPlayer rpgPlayer, string message)
    {
        _overlayNotificationsService.AddNotification(rpgPlayer, message);
    }
}

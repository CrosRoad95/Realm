using Realm.Resources.Overlay.Events;
using SlipeServer.Server.Elements;

namespace Realm.Resources.Overlay;

public class OverlayNotificationsService
{
    internal event Action<AddNotificationEvent>? NotificationAdded;

    public void AddNotification(Player player, string message)
    {
        NotificationAdded?.Invoke(new AddNotificationEvent(player, message));
    }
}

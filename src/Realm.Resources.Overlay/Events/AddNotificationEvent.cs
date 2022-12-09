using SlipeServer.Server.Elements;

namespace Realm.Resources.Overlay.Events;

internal class AddNotificationEvent
{

    public Player Player { get; }
    public string Message { get; }

    public AddNotificationEvent(Player player, string message)
    {
        Player = player;
        Message = message;
    }
}

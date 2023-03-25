using SlipeServer.Server.Elements;

namespace Realm.Resources.AFK;

public interface IAFKService
{
    event Action<Player>? PlayerAFKStarted;
    event Action<Player>? PlayerAFKStopped;

    internal void HandleAFKStart(Player player);
    internal void HandleAFKStop(Player player);

    bool IsAFK(Player player);
}

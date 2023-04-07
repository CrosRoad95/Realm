using SlipeServer.Server.Elements;

namespace RealmCore.Resources.AFK;

internal sealed class AFKService : IAFKService
{
    public event Action<Player>? PlayerAFKStarted;
    public event Action<Player>? PlayerAFKStopped;
    private readonly HashSet<Player> _afkPlayers = new();

    public AFKService()
    {
    }

    public bool IsAFK(Player player) => _afkPlayers.Contains(player);

    public void HandleAFKStart(Player player)
    {
        if (_afkPlayers.Add(player))
            PlayerAFKStarted?.Invoke(player);
    }

    public void HandleAFKStop(Player player)
    {
        if (_afkPlayers.Remove(player))
            PlayerAFKStopped?.Invoke(player);
    }
}

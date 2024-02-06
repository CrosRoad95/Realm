using SlipeServer.Server.Elements;

namespace RealmCore.Resources.AFK;

public interface IAFKService
{
    event Action<Player>? PlayerAFKStarted;
    event Action<Player>? PlayerAFKStopped;

    internal void HandleAFKStart(Player player);
    internal void HandleAFKStop(Player player);

    bool IsAFK(Player player);
}

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

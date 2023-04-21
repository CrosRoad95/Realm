using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;

namespace RealmCore.Resources.Admin;

internal sealed class AdminService : IAdminService
{
    public event Action<Player>? AdminEnabled;
    public event Action<Player>? AdminDisabled;

    private readonly HashSet<Player> _enabledForPlayers = new();
    private readonly object _lock = new();
    public AdminService()
    {

    }

    public bool HasAdminModeEnabled(Player player)
    {
        lock (_lock)
            return _enabledForPlayers.Contains(player);
    }

    public void EnableAdminModeForPlayer(Player player)
    {
        bool succeed;
        lock (_lock)
            succeed = _enabledForPlayers.Add(player);

        if (succeed)
        {
            AdminEnabled?.Invoke(player);
            player.Disconnected += HandlePlayerDisconnected;
        }
    }

    private void HandlePlayerDisconnected(Player player, PlayerQuitEventArgs e)
    {
        lock (_lock)
            _enabledForPlayers.Remove(player);
    }

    public void DisableAdminModeForPlayer(Player player)
    {
        bool succeed;
        lock (_lock)
            succeed = _enabledForPlayers.Remove(player);

        if (succeed)
        {
            AdminDisabled?.Invoke(player);
            player.Disconnected -= HandlePlayerDisconnected;
        }
    }
}

using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;

namespace RealmCore.Resources.AdminTools;

internal sealed class AdminToolsService : IAdminToolsService
{
    public event Action<Player>? AdminToolsEnabled;
    public event Action<Player>? AdminToolsDisabled;

    private readonly HashSet<Player> _enabledForPlayers = new();
    private readonly object _lock = new();
    public AdminToolsService()
    {

    }

    public bool HasAdminToolsEnabled(Player player)
    {
        return _enabledForPlayers.Contains(player);
    }

    public void EnableAdminToolsForPlayer(Player player)
    {
        bool succeed;
        lock (_lock)
            succeed = _enabledForPlayers.Add(player);

        if (succeed)
        {
            AdminToolsEnabled?.Invoke(player);
            player.Disconnected += HandlePlayerDisconnected;
        }
    }

    private void HandlePlayerDisconnected(Player player, PlayerQuitEventArgs e)
    {
        lock (_lock)
            _enabledForPlayers.Remove(player);
    }

    public void DisableAdminToolsForPlayer(Player player)
    {
        bool succeed;
        lock (_lock)
            succeed = _enabledForPlayers.Remove(player);

        if (succeed)
        {
            AdminToolsDisabled?.Invoke(player);
            player.Disconnected -= HandlePlayerDisconnected;
        }
    }
}

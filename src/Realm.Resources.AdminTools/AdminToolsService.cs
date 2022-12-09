using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;

namespace Realm.Resources.AdminTools;

public class AdminToolsService
{
    internal event Action<Player>? AdminToolsEnabled;
    internal event Action<Player>? AdminToolsDisabled;

    private readonly HashSet<Player> _enabledForPlayers = new();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
    public AdminToolsService()
    {

    }

    public bool HasAdminToolsEnabled(Player player)
    {
        return _enabledForPlayers.Contains(player);
    }

    public void EnableAdminToolsForPlayer(Player player)
    {
        _semaphoreSlim.Wait();
        bool succeed = _enabledForPlayers.Add(player);

        _semaphoreSlim.Release();

        if(succeed)
        {
            AdminToolsEnabled?.Invoke(player);
            player.Disconnected += HandlePlayerDisconnected;
        }
    }

    private void HandlePlayerDisconnected(Player player, PlayerQuitEventArgs e)
    {
        _semaphoreSlim.Wait();
        _enabledForPlayers.Remove(player);
        _semaphoreSlim.Release();
    }

    public void DisableAdminToolsForPlayer(Player player)
    {
        _semaphoreSlim.Wait();
        bool succeed = _enabledForPlayers.Remove(player);
        _semaphoreSlim.Release();

        if(succeed)
        {
            AdminToolsDisabled?.Invoke(player);
            player.Disconnected -= HandlePlayerDisconnected;
        }
    }
}

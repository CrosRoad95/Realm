using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;

namespace Realm.Resources.AFK;

public class AFKService
{
    public event Action<Player>? PlayerAFKStarted;
    public event Action<Player>? PlayerAFKSStopped;
    private readonly HashSet<Player> _afkPlayers = new();

    public AFKService(LuaEventService luaEventService)
    {
    }

    public bool IsAFK(Player player) => _afkPlayers.Contains(player);

    internal void HandleAFKStart(Player player)
    {
        if(_afkPlayers.Add(player))
            PlayerAFKStarted?.Invoke(player);
    }

    internal void HandleAFKStop(Player player)
    {
        if(_afkPlayers.Remove(player))
            PlayerAFKSStopped?.Invoke(player);
    }
}

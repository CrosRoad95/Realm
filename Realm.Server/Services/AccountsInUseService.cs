using Realm.Interfaces.Server.Services;
using System.Collections.Concurrent;

namespace Realm.Server.Services;

public class AccountsInUseService : IAccountsInUseService
{
    private readonly ConcurrentDictionary<string, RPGPlayer> _playerByAccountId = new();
    public AccountsInUseService()
    {

    }

    public RPGPlayer GetPlayerByAccountId(string id)
    {
        return _playerByAccountId[id];
    }
    
    public bool IsAccountIdInUse(string id)
    {
        return _playerByAccountId.ContainsKey(id);
    }

    public bool AssignPlayerToAccountId(RPGPlayer player, string id)
    {
        player.LoggedOut += Player_LoggedOut;
        return _playerByAccountId.TryAdd(id, player);
    }

    private void Player_LoggedOut(RPGPlayer rpgPlayer, string id)
    {
        FreeAccountId(id);
    }

    private bool FreeAccountId(string id)
    {
        return _playerByAccountId.TryRemove(id, out var _);
    }
}

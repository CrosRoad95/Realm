using System.Collections.Concurrent;

namespace Realm.Server.Services;

public class AccountsInUseService
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
        return _playerByAccountId.TryAdd(id, player);
    }

    public bool FreeAccountId(string id)
    {
        return _playerByAccountId.TryRemove(id, out var _);
    }
}

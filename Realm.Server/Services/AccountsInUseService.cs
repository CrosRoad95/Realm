using System.Collections.Concurrent;

namespace Realm.Server.Services;

public class AccountsInUseService : IAccountsInUseService
{
    private readonly ConcurrentDictionary<string, RPGPlayer> _playerByAccountId = new();
    private readonly ILogger _logger;

    public AccountsInUseService(ILogger logger)
    {
        _logger = logger.ForContext<AccountsInUseService>();
    }

    public RPGPlayer GetPlayerByAccountId(string id)
    {
        return _playerByAccountId[id];
    }
    
    public bool TryGetPlayerByAccountId(string id, out RPGPlayer? rpgPlayer)
    {
        return _playerByAccountId.TryGetValue(id, out rpgPlayer);
    }
    
    public bool IsAccountIdInUse(string id)
    {
        return _playerByAccountId.ContainsKey(id);
    }

    public bool AssignPlayerToAccountId(RPGPlayer player, string id)
    {
        player.LoggedOut += Player_LoggedOut;
        var success = _playerByAccountId.TryAdd(id, player);
        if (success)
            _logger.Verbose("Locked player {player} to {id} account id.", player, id);
        else
            _logger.Verbose("Failed to lock player {player} to {id} account id.", player, id);
        return success;
    }

    private void Player_LoggedOut(RPGPlayer rpgPlayer, string id)
    {
        FreeAccountId(id);
    }

    private bool FreeAccountId(string id)
    {
        var success = _playerByAccountId.TryRemove(id, out var player);
        if (success)
            _logger.Verbose("Unlocked account of id {id} locked by player {player}.", id, player);
        else
            _logger.Verbose("Failed to unlock  account of id {id}. Possible bug.", id);
        return success;
    }
}

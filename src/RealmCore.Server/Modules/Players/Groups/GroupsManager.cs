namespace RealmCore.Server.Modules.Players.Groups;

public sealed class GroupsManager
{
    private readonly object _lock = new();
    private readonly Dictionary<int, List<RealmPlayer>> _playersByGroupId = [];
    private readonly Dictionary<RealmPlayer, List<int>> _groupsIdsByPlayer = [];

    public void AddPlayerToGroup(int groupId, RealmPlayer player)
    {
        lock (_lock)
        {
            if(_groupsIdsByPlayer.TryGetValue(player, out var groups))
            {
                groups.Add(groupId);
            }
            else
            {
                _groupsIdsByPlayer[player] = [groupId];
            }
            if(_playersByGroupId.TryGetValue(groupId, out var players))
            {
                players.Add(player);
            }
            else
            {
                _playersByGroupId[groupId] = [player];
            }

        }
    }

    public void RemovePlayerFromAllGroups(RealmPlayer player)
    {
        lock (_lock)
        {
            if(_groupsIdsByPlayer.TryGetValue(player, out var groups))
            {
                foreach (var groupId in groups)
                {
                    if (_playersByGroupId.TryGetValue(groupId, out var players))
                    {
                        players.Remove(player);
                        if (players.Count == 0)
                            _playersByGroupId.Remove(groupId);
                    }
                    _groupsIdsByPlayer.Remove(player);
                }
            }
        }
    }
    
    public void RemovePlayerFromGroup(int groupId, RealmPlayer player)
    {
        lock (_lock)
        {
            _groupsIdsByPlayer.Remove(player);
            if(_playersByGroupId.TryGetValue(groupId, out var players))
            {
                players.Remove(player);
                if(players.Count == 0)
                    _playersByGroupId.Remove(groupId);
            }
        }
    }

    public IEnumerable<RealmPlayer> GetPlayersInGroup(int groupId)
    {
        RealmPlayer[] view = [];
        lock (_lock)
        {
            if (_playersByGroupId.TryGetValue(groupId, out var players))
            {
                view = [.. players];
            }
        }

        foreach (var player in view)
        {
            yield return player;
        }
    }

    public int[] GetGroupsByPlayer(RealmPlayer player)
    {
        lock (_lock)
        {
            if (_groupsIdsByPlayer.TryGetValue(player, out var groupsIds))
                return [.. groupsIds];
            return [];
        }
    }
}

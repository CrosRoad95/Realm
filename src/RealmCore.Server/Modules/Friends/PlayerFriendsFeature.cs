using static RealmCore.Server.Modules.Friends.FriendsResults;

namespace RealmCore.Server.Modules.Friends;

public sealed class PlayerFriendsFeature : IPlayerFeature, IEnumerable<int>
{
    private readonly object _lock = new();

    public RealmPlayer Player { get; init; }

    private readonly List<int> _friends = [];
    private readonly List<int> _pendingOutgoingFriendRequests = [];
    private readonly List<int> _pendingIncomingFriendRequests = [];

    public event Action<int>? Removed;
    public event Action<int>? Added;
    public event Action<int>? ReceivedFriendRequest;
    public event Action<int>? RemovedReceivedFriendRequest;
    public event Action<int>? SentFriendRequest;
    public event Action<int>? RemovedSentFriendRequest;

    private readonly PlayerUserFeature _playerUserFeature;
    private readonly FriendsService _friendsService;
    private readonly IUsersInUse _usersInUse;

    public PlayerFriendsFeature(PlayerContext playerContext, PlayerUserFeature playerUserFeature, FriendsService friendsService, IUsersInUse usersInUse)
    {
        Player = playerContext.Player;
        _playerUserFeature = playerUserFeature;
        _friendsService = friendsService;
        _usersInUse = usersInUse;
    }
    
    public int[] GetPendingOutgoingRequests()
    {
        lock (_lock)
            return [.. _pendingOutgoingFriendRequests];
    }
    
    public int[] GetPendingIncomingRequests()
    {
        lock (_lock)
            return [.. _pendingIncomingFriendRequests];
    }
    
    public List<RealmPlayer> GetOnline()
    {
        List<RealmPlayer> onlineFriends = new();
        lock (_lock)
        {
            foreach (var friend in _friends)
            {
                if(_usersInUse.TryGetPlayerByUserId(friend, out var player))
                {
                    onlineFriends.Add(player);
                }
            }
        }

        return onlineFriends;
    }

    public async Task<bool> Remove(int userId)
    {
        var id = _playerUserFeature.Id;
        var result = await _friendsService.RemoveFriend(id, userId);

        if (result.Value is not FriendRemoved)
            return false;

        lock (_lock)
        {
            _friends.Remove(userId);
        }

        Removed?.Invoke(userId);

        if (_usersInUse.TryGetPlayerByUserId(userId, out var player) && player != null)
        {
            player.Friends.InternalRemoveFriend(id);
        }

        return true;
    }

    public async Task<bool> SendRequest(int userId)
    {
        var id = _playerUserFeature.Id;
        var result = await _friendsService.SendFriendRequest(id, userId);

        if (result.Value is not RequestSent)
            return false;

        lock (_lock)
        {
            _pendingOutgoingFriendRequests.Add(userId);
        }

        SentFriendRequest?.Invoke(userId);

        if (_usersInUse.TryGetPlayerByUserId(userId, out var player) && player != null)
        {
            player.Friends.InternalAddIncomingFriendRequest(id);
        }

        return true;
    }

    public async Task<bool> AcceptRequest(int userId)
    {
        var id = _playerUserFeature.Id;
        var result = await _friendsService.AcceptFriendRequest(id, userId);

        if(result.Value is not RequestAccepted)
            return false;

        lock (_lock)
        {
            _pendingOutgoingFriendRequests.Remove(userId);
            _friends.Add(userId);
        }

        SentFriendRequest?.Invoke(userId);

        return true;
    }

    public async Task<bool> RejectRequest(int userId)
    {
        var id = _playerUserFeature.Id;
        var result = await _friendsService.RejectFriendRequest(id, userId);

        if (result.Value is not FriendRequestRejected)
            return false;

        bool removedIncoming;
        bool removedOutgoing;
        lock (_lock)
        {
            removedIncoming = _pendingIncomingFriendRequests.Remove(userId);
            removedOutgoing = _pendingOutgoingFriendRequests.Remove(userId);
        }

        if (_usersInUse.TryGetPlayerByUserId(userId, out var player) && player != null)
        {
            if (removedIncoming)
                player.Friends.InternalRemoveReceivedFriendRequest(id);
            if (removedOutgoing)
                player.Friends.InternalRemoveSentFriendRequest(id);
        }

        if(removedIncoming)
            RemovedReceivedFriendRequest?.Invoke(userId);
        if(removedOutgoing)
            RemovedSentFriendRequest?.Invoke(userId);

        return true;
    }

    public void InternalAddIncomingFriendRequest(int userId)
    {
        lock (_lock)
        {
            _pendingIncomingFriendRequests.Add(userId);
        }

        ReceivedFriendRequest?.Invoke(userId);
    }

    public void InternalAddFriend(int userId)
    {
        lock (_lock)
        {
            _friends.Add(userId);
            _pendingIncomingFriendRequests.Remove(userId);
            _pendingOutgoingFriendRequests.Remove(userId);
        }

        Added?.Invoke(userId);
    }

    public void InternalRemoveFriend(int userId)
    {
        lock (_lock)
        {
            _friends.Remove(userId);
        }

        Removed?.Invoke(userId);
    }

    public void InternalRemoveReceivedFriendRequest(int userId)
    {
        lock (_lock)
        {
            _pendingIncomingFriendRequests.Remove(userId);
        }

        RemovedReceivedFriendRequest?.Invoke(userId);
    }

    public void InternalRemoveSentFriendRequest(int userId)
    {
        lock (_lock)
        {
            _pendingOutgoingFriendRequests.Remove(userId);
        }

        RemovedSentFriendRequest?.Invoke(userId);
    }

    public IEnumerator<int> GetEnumerator()
    {
        int[] view;
        {
            lock (_lock)
                view = [.. _friends];
        }

        foreach (var friend in view)
        {
            yield return friend;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

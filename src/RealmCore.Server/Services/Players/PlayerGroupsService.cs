namespace RealmCore.Server.Services.Players;

public interface IPlayerGroupsService : IPlayerService
{
    internal bool AddGroupMember(GroupMemberData groupMemberData);
    internal bool RemoveGroupMember(int groupId);
    bool IsMember(int groupId);
    GroupMemberData? GetMemberOrDefault(int groupId);
}

internal sealed class PlayerGroupsService : IPlayerGroupsService
{
    private readonly SemaphoreSlim _lock = new(1);
    private readonly IPlayerUserService _playerUserService;
    private ICollection<GroupMemberData> _groupMembers = [];
    public RealmPlayer Player { get; private set; }
    public PlayerGroupsService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        _lock.Wait();
        try
        {
            _groupMembers = playerUserService.User.GroupMembers;
        }
        finally
        {
            _lock.Release();
        }
    }

    public bool AddGroupMember(GroupMemberData groupMemberData)
    {
        _lock.Wait();
        try
        {
            var group = _groupMembers.FirstOrDefault(x => x.GroupId == groupMemberData.GroupId);
            if (group == null)
                return false;

            _groupMembers.Add(groupMemberData);
        }
        finally
        {
            _lock.Release();
        }
        _playerUserService.IncreaseVersion();
        return true;
    }

    public bool RemoveGroupMember(int groupId)
    {
        _lock.Wait();
        try
        {
            var group = _groupMembers.FirstOrDefault(x => x.GroupId == groupId);
            if (group == null)
                return false;
            _groupMembers.Remove(group);
        }
        finally
        {
            _lock.Release();
        }
        _playerUserService.IncreaseVersion();
        return true;
    }

    public bool IsMember(int groupId)
    {
        _lock.Wait();
        try
        {
            return _groupMembers.Any(x => x.GroupId == groupId);
        }
        finally
        {
            _lock.Release();
        }
    }
    

    public GroupMemberData? GetMemberOrDefault(int groupId)
    {
        _lock.Wait();
        try
        {
            return _groupMembers.FirstOrDefault(x => x.GroupId == groupId);
        }
        finally
        {
            _lock.Release();
        }
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {

    }
}

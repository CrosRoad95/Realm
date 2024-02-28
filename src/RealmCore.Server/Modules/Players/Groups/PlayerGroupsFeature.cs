namespace RealmCore.Server.Modules.Players.Groups;

public interface IPlayerGroupsFeature : IPlayerFeature
{
    internal bool AddGroupMember(GroupMemberData groupMemberData);
    internal bool RemoveGroupMember(int groupId);
    bool IsMember(int groupId);
    GroupMemberData? GetMemberOrDefault(int groupId);
}

internal sealed class PlayerGroupsFeature : IPlayerGroupsFeature, IUsesUserPersistentData
{
    private readonly SemaphoreSlim _lock = new(1);
    private ICollection<GroupMemberData> _groupMembers = [];

    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }
    public PlayerGroupsFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void SignIn(UserData userData)
    {
        _lock.Wait();
        try
        {
            _groupMembers = userData.GroupMembers;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void SignOut()
    {
        _lock.Wait();
        try
        {
            _groupMembers = [];
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
            if (group != null)
                return false;

            _groupMembers.Add(groupMemberData);
        }
        finally
        {
            _lock.Release();
        }

        VersionIncreased?.Invoke();
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

        VersionIncreased?.Invoke();
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

}

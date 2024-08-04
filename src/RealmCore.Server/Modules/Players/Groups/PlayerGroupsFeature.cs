
namespace RealmCore.Server.Modules.Players.Groups;

public sealed class PlayerGroupsFeature : IPlayerFeature, IEnumerable<GroupMemberDto>, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private ICollection<GroupMemberData> _groupMembers = [];

    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }

    public event Action<PlayerGroupsFeature, GroupMemberDto>? Added;
    public event Action<PlayerGroupsFeature, GroupMemberDto>? Removed;
    public PlayerGroupsFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void LogIn(UserData userData)
    {
        lock(_lock)
            _groupMembers = userData.GroupMembers;
    }

    internal bool AddGroupMember(GroupMemberData groupMemberData)
    {
        lock (_lock)
        {
            var group = _groupMembers.FirstOrDefault(x => x.GroupId == groupMemberData.GroupId);
            if (group != null)
                return false;

            _groupMembers.Add(groupMemberData);
        }
        VersionIncreased?.Invoke();
        Added?.Invoke(this, GroupMemberDto.Map(groupMemberData));
        return true;
    }

    internal bool RemoveGroupMember(int groupId)
    {
        GroupMemberData? groupMemberData = null;
        lock (_lock)
        {
            groupMemberData = _groupMembers.FirstOrDefault(x => x.GroupId == groupId);
            if (groupMemberData == null)
                return false;
            _groupMembers.Remove(groupMemberData);
        }

        VersionIncreased?.Invoke();
        Added?.Invoke(this, GroupMemberDto.Map(groupMemberData));
        return true;
    }

    public bool IsMember(int groupId)
    {
        lock (_lock)
            return _groupMembers.Any(x => x.GroupId == groupId);
    }

    public bool TryGetMember(int groupId, out GroupMemberDto groupMember)
    {
        lock (_lock)
        {
            groupMember = GroupMemberDto.Map(_groupMembers.FirstOrDefault(x => x.GroupId == groupId));
            return groupMember != null;
        }
    }

    public IEnumerator<GroupMemberDto> GetEnumerator()
    {
        GroupMemberDto[] view;
        lock (_lock)
            view = [.. _groupMembers.Select(GroupMemberDto.Map)];

        foreach (var groupMemberDto in view)
            yield return groupMemberDto;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal void Clear()
    {
        lock (_lock)
            _groupMembers.Clear();
    }
}

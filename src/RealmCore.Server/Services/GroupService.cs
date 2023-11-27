namespace RealmCore.Server.Services;

internal sealed class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;

    public GroupService(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    private Group Map(GroupData groupData)
    {
        return new Group
        {
            id = groupData.Id,
            name = groupData.Name,
            shortcut = groupData.Shortcut,
            kind = (GroupKind)(groupData.Kind ?? 0),
            members = groupData.Members.Select(x => new GroupMember
            {
                userId = x.UserId,
                rank = x.Rank,
                rankName = x.RankName
            }).ToArray()
        };
    }

    public async Task<Group?> GetGroupByName(string groupName, CancellationToken cancellationToken = default)
    {
        var groupData = await _groupRepository.GetByName(groupName, cancellationToken);
        if (groupData == null)
            return null;

        return Map(groupData);
    }

    public async Task<Group?> GetGroupByNameOrShortCut(string groupName, string shortcut, CancellationToken cancellationToken = default)
    {
        var groupData = await _groupRepository.GetGroupByNameOrShortcut(groupName, shortcut, cancellationToken);
        if (groupData == null)
            return null;

        return Map(groupData);
    }

    public async Task<bool> GroupExistsByNameOrShorCut(string groupName, string shortcut, CancellationToken cancellationToken = default)
    {
        return await _groupRepository.ExistsByNameOrShortcut(groupName, shortcut, cancellationToken);
    }

    public async Task<Group> CreateGroup(string groupName, string shortcut, GroupKind groupKind = GroupKind.Regular, CancellationToken cancellationToken = default)
    {
        if (await _groupRepository.ExistsByName(groupName, cancellationToken))
            throw new GroupNameInUseException(groupName);

        if (await _groupRepository.ExistsByShortcut(shortcut, cancellationToken))
            throw new GroupShortcutInUseException(shortcut);

        var groupData = await _groupRepository.Create(groupName, shortcut, (byte)groupKind, cancellationToken);
        return Map(groupData);
    }

    public async Task<bool> AddMember(RealmPlayer player, int groupId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default)
    {
        if (player.Components.HasComponent<GroupMemberComponent>(x => x.GroupId == groupId))
            return false;

        var groupMemberData = await _groupRepository.AddMember(groupId, player.UserId, rank, rankName, cancellationToken);
        player.AddComponent(new GroupMemberComponent(groupMemberData));
        return true;
    }

    public async Task<bool> RemoveMember(RealmPlayer player, int groupId, CancellationToken cancellationToken = default)
    {
        var groupMemberComponent = player.Components.FindComponent<GroupMemberComponent>(x => x.GroupId == groupId);
        if (groupMemberComponent == null)
            return false;

        if (await _groupRepository.RemoveMember(groupId, player.UserId, cancellationToken))
        {
            return player.Components.TryDestroyComponent(groupMemberComponent);
        }
        return false;
    }
}

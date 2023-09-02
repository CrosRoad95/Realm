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
            kind = (GroupKind)groupData.Kind,
            members = groupData.Members.Select(x => new GroupMember
            {
                userId = x.UserId,
                rank = x.Rank,
                rankName = x.RankName
            }).ToArray()
        };
    }

    public async Task<Group?> GetGroupByName(string groupName)
    {
        var groupData = await _groupRepository.GetByName(groupName).ConfigureAwait(false);
        if (groupData == null)
            return null;

        return Map(groupData);
    }

    public async Task<Group?> GetGroupByNameOrShorcut(string groupName, string shortcut)
    {
        var groupData = await _groupRepository.GetGroupByNameOrShortcut(groupName, shortcut).ConfigureAwait(false);
        if (groupData == null)
            return null;

        return Map(groupData);
    }

    public async Task<bool> GroupExistsByNameOrShorcut(string groupName, string shortcut)
    {
        return await _groupRepository.ExistsByNameOrShortcut(groupName, shortcut).ConfigureAwait(false);
    }

    public async Task<Group> CreateGroup(string groupName, string shortcut, GroupKind groupKind = GroupKind.Regular)
    {
        if (await _groupRepository.ExistsByName(groupName))
            throw new GroupNameInUseException(groupName);

        if (await _groupRepository.ExistsByShortcut(shortcut))
            throw new GroupShortcutInUseException(shortcut);

        var groupData = await _groupRepository.Create(groupName, shortcut, (byte)groupKind).ConfigureAwait(false);
        return Map(groupData);
    }

    public async Task<bool> AddMember(Entity entity, int groupId, int rank = 1, string rankName = "")
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            throw new InvalidOperationException();

        if (entity.TryGetComponent(out UserComponent userComponent))
        {
            if (entity.HasComponent<GroupMemberComponent>(x => x.GroupId == groupId))
                return false;

            var groupMemberData = await _groupRepository.AddMember(groupId, userComponent.Id, rank, rankName).ConfigureAwait(false);
            entity.AddComponent(new GroupMemberComponent(groupMemberData));
        }
        return false;
    }

    public bool IsUserInGroup(Entity entity, int groupId)
    {
        return entity.HasComponent<GroupMemberComponent>(x => x.GroupId == groupId);
    }

    public async Task<bool> RemoveMember(Entity entity, int groupId)
    {
        if (entity.TryGetComponent(out UserComponent userComponent))
        {
            var groupMemberComponent = entity.FindComponent<GroupMemberComponent>(x => x.GroupId == groupId);
            if (groupMemberComponent == null)
                return false;

            if (await _groupRepository.RemoveMember(groupId, userComponent.Id).ConfigureAwait(false))
            {
                return entity.TryDestroyComponent(groupMemberComponent);
            }
        }
        return false;
    }
}

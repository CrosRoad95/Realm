using RealmCore.Persistance.Interfaces;
using Group = RealmCore.Server.Concepts.Group;
using GroupData = RealmCore.Persistance.Data.GroupData;
using GroupMember = RealmCore.Server.Concepts.GroupMember;

namespace RealmCore.Server.Services;

internal class GroupService : IGroupService
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
        var groupData = await _groupRepository.GetGroupByName(groupName);
        if (groupData == null)
            return null;

        return Map(groupData);
    }

    public async Task<Group?> GetGroupByNameOrShortut(string groupName, string shortcut)
    {
        var groupData = await _groupRepository.GetGroupByNameOrShortcut(groupName, shortcut);
        if (groupData == null)
            return null;

        return Map(groupData);
    }

    public Task<bool> GroupExistsByNameOrShortut(string groupName, string shortcut)
    {
        return _groupRepository.ExistsByNameOrShortcut(groupName, shortcut);
    }

    public async Task<Group> CreateGroup(string groupName, string shortcut, GroupKind groupKind = GroupKind.Regular)
    {
        if (await _groupRepository.ExistsByName(groupName))
            throw new GroupNameInUseException(groupName);

        if (await _groupRepository.ExistsByShortcut(shortcut))
            throw new GroupShortcutInUseException(shortcut);

        var groupData = await _groupRepository.CreateNewGroup(groupName, shortcut, (byte)groupKind);
        return Map(groupData);
    }

    public async Task AddMember(int groupId, Entity entity, int rank = 1, string rankName = "")
    {
        if (entity.Tag != EntityTag.Player)
            throw new InvalidOperationException();

        var userId = entity.GetRequiredComponent<UserComponent>().Id;
        var groupMemberData = await _groupRepository.CreateNewGroupMember(groupId, userId, rank, rankName);
        entity.AddComponent(new GroupMemberComponent(groupMemberData));
    }

    public async Task AddMember(string groupName, Entity entity, int rank = 1, string rankName = "")
    {
        if (entity.Tag != EntityTag.Player)
            throw new InvalidOperationException();

        var userId = entity.GetRequiredComponent<UserComponent>().Id;
        var groupMemberData = await _groupRepository.CreateNewGroupMember(groupName, userId, rank, rankName);
        entity.AddComponent(new GroupMemberComponent(groupMemberData));
    }

    public async Task AddMember(string groupName, int userId, int rank = 1, string rankName = "")
    {
        await _groupRepository.CreateNewGroupMember(groupName, userId, rank, rankName);
    }

    public async Task AddMember(int groupId, int userId, int rank = 1, string rankName = "")
    {
        await _groupRepository.CreateNewGroupMember(groupId, userId, rank, rankName);
    }

    public async Task RemoveMember(int groupId, int userId)
    {
        if (!await _groupRepository.RemoveGroupMember(groupId, userId))
            throw new GroupMemberNotFoundException(groupId, userId);
    }
}

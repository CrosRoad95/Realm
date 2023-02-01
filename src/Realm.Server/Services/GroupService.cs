using Realm.Domain.Enums;
using Realm.Domain.Exceptions;
using Realm.Persistance.Interfaces;
using Group = Realm.Domain.Concepts.Group;
using GroupData = Realm.Persistance.Data.Group;
using GroupMember = Realm.Domain.Concepts.GroupMember;

namespace Realm.Server.Services;

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
    
    public async Task<Group> CreateGroup(string groupName, string shortcut, GroupKind groupKind = GroupKind.Regular)
    {
        if(await _groupRepository.ExistsByName(groupName))
            throw new GroupNameInUseException(groupName);
        
        if(await _groupRepository.ExistsByShortcut(shortcut))
            throw new GroupShortcutInUseException(shortcut);

        var groupData = await _groupRepository.CreateNewGroup(groupName, shortcut, (byte)groupKind);
        return Map(groupData);
    }

    public async Task AddMember(int groupId, Entity entity, int rank = 1, string rankName = "")
    {
        if (entity.Tag != Entity.EntityTag.Player)
            throw new InvalidOperationException();

        var userId = entity.GetRequiredComponent<AccountComponent>().Id;
        var groupMemberData = await _groupRepository.CreateNewGroupMember(groupId, userId, rank, rankName);
        entity.AddComponent(new GroupMemeberComponent(groupMemberData));
    }

    public async Task AddMember(string groupName, Entity entity, int rank = 1, string rankName = "")
    {
        if (entity.Tag != Entity.EntityTag.Player)
            throw new InvalidOperationException();

        var userId = entity.GetRequiredComponent<AccountComponent>().Id;
        var groupMemberData = await _groupRepository.CreateNewGroupMember(groupName, userId, rank, rankName);
        entity.AddComponent(new GroupMemeberComponent(groupMemberData));
    }

    public async Task AddMember(string groupName, Guid userId, int rank = 1, string rankName = "")
    {
        await _groupRepository.CreateNewGroupMember(groupName, userId, rank, rankName);
    }

    public async Task AddMember(int groupId, Guid userId, int rank = 1, string rankName = "")
    {
        await _groupRepository.CreateNewGroupMember(groupId, userId, rank, rankName);
    }

    public async Task RemoveMember(int groupId, Guid userId)
    {
        if (!await _groupRepository.RemoveGroupMember(groupId, userId))
            throw new GroupMemberNotFoundException(groupId, userId);
    }
}

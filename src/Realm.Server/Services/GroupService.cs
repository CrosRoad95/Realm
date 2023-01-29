using Realm.Domain.Concepts;
using Realm.Domain.Enums;
using Realm.Persistance.Data;
using Realm.Persistance.Interfaces;
using Group = Realm.Domain.Concepts.Group;
using GroupData = Realm.Persistance.Data.Group;

namespace Realm.Server.Services;

internal class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;

    public GroupService(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<Group> CreateGroup(string groupName, string shortcut, GroupKind groupKind = GroupKind.Regular)
    {
        var group = await _groupRepository.CreateNewGroup(groupName, shortcut, (byte)groupKind);
        return new Group
        {
            id = group.Id,
            name = group.Name,
            shortcut = group.Shortcut,
            kind = (GroupKind)group.Kind,
        };
    }

    public async Task AddMember(int groupId, Entity entity, int rank = 1, string rankName = "")
    {
        if (entity.Tag != Entity.PlayerTag)
            throw new InvalidOperationException();

        var userId = entity.GetRequiredComponent<AccountComponent>().Id;
        var groupMemberData = await _groupRepository.CreateNewGroupMember(groupId, userId, rank, rankName);
        entity.AddComponent(new GroupMemeberComponent(groupMemberData));
    }

    public async Task AddMember(string groupName, Entity entity, int rank = 1, string rankName = "")
    {
        if (entity.Tag != Entity.PlayerTag)
            throw new InvalidOperationException();

        var userId = entity.GetRequiredComponent<AccountComponent>().Id;
        var groupMemberData = await _groupRepository.CreateNewGroupMember(groupName, userId, rank, rankName);
        entity.AddComponent(new GroupMemeberComponent(groupMemberData));
    }
}

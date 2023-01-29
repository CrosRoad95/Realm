using Realm.Domain.Enums;

namespace Realm.Server.Interfaces;

public interface IGroupService
{
    Task<Domain.Concepts.Group> CreateGroup(string groupName, string shortcut, GroupKind groupKind = GroupKind.Regular);
    Task AddMember(string groupName, Entity entity, int rank = 1, string rankName = "");
    Task AddMember(int groupId, Entity entity, int rank = 1, string rankName = "");
}

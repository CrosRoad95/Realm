namespace Realm.Persistance.Interfaces;

public interface IGroupRepository : IDisposable
{
    Task<Group> CreateNewGroup(string groupName, string shortcut, byte kind = 1);
    Task<GroupMember> CreateNewGroupMember(string groupName, Guid userId, int rank = 1, string rankName = "");
    Task<GroupMember> CreateNewGroupMember(int groupId, Guid userId, int rank = 1, string rankName = "");
}

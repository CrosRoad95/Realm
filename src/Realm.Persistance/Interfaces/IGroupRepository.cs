namespace Realm.Persistance.Interfaces;

public interface IGroupRepository : IRepositoryBase
{
    Task<Group> CreateNewGroup(string groupName, string shortcut, byte kind = 1);
    Task<GroupMember> CreateNewGroupMember(string groupName, int userId, int rank = 1, string rankName = "");
    Task<GroupMember> CreateNewGroupMember(int groupId, int userId, int rank = 1, string rankName = "");
    Task<bool> ExistsByName(string groupName);
    Task<bool> ExistsByNameOrShortcut(string groupName, string shortcut);
    Task<bool> ExistsByShortcut(string shortcut);
    Task<Group?> GetGroupByName(string groupName);
    Task<Group?> GetGroupByNameOrShortcut(string groupName, string shortcut);
    Task<bool> RemoveGroupMember(int groupId, int userId);
}

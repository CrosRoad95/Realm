namespace RealmCore.Persistance.Interfaces;

public interface IGroupRepository : IRepositoryBase
{
    Task<GroupData> CreateNewGroup(string groupName, string shortcut, byte kind = 1);
    Task<GroupMemberData> CreateNewGroupMember(string groupName, int userId, int rank = 1, string rankName = "");
    Task<GroupMemberData> CreateNewGroupMember(int groupId, int userId, int rank = 1, string rankName = "");
    Task<bool> ExistsByName(string groupName);
    Task<bool> ExistsByNameOrShortcut(string groupName, string shortcut);
    Task<bool> ExistsByShortcut(string shortcut);
    Task<GroupData?> GetGroupByName(string groupName);
    Task<GroupData?> GetGroupByNameOrShortcut(string groupName, string shortcut);
    Task<bool> RemoveGroupMember(int groupId, int userId);
}

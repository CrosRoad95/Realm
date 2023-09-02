namespace RealmCore.Persistence.Interfaces;

public interface IGroupRepository
{
    Task<GroupData> Create(string groupName, string shortcut, byte kind = 1);
    Task<GroupMemberData> AddMember(int groupId, int userId, int rank = 1, string rankName = "");
    Task<bool> ExistsByName(string groupName);
    Task<bool> ExistsByNameOrShortcut(string groupName, string shortcut);
    Task<bool> ExistsByShortcut(string shortcut);
    Task<GroupData?> GetByName(string groupName);
    Task<GroupData?> GetGroupByNameOrShortcut(string groupName, string shortcut);
    Task<bool> IsUserInGroup(int groupId, int userId);
    Task<bool> RemoveMember(int groupId, int userId);
}

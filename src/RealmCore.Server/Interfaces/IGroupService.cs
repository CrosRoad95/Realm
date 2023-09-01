using RealmCore.Server.Structs;

namespace RealmCore.Server.Interfaces;

public interface IGroupService
{
    Task<Group> CreateGroup(string groupName, string shortcut, GroupKind groupKind = GroupKind.Regular);
    Task AddMember(string groupName, Entity entity, int rank = 1, string rankName = "");
    Task AddMember(int groupId, Entity entity, int rank = 1, string rankName = "");
    Task AddMember(string groupName, int userId, int rank = 1, string rankName = "");
    Task<Group?> GetGroupByName(string groupName);
    Task AddMember(int groupId, int userId, int rank = 1, string rankName = "");
    Task RemoveMember(int groupId, int userId);
    Task<bool> GroupExistsByNameOrShorcut(string groupName, string shortcut);
    Task<Group?> GetGroupByNameOrShorcut(string groupName, string shortcut);
    Task<bool> IsUserInGroup(int groupId, int userId);
}

namespace RealmCore.Server.Interfaces;

public interface IGroupService
{
    Task<bool> AddMember(Entity entity, int groupId, int rank = 1, string rankName = "");
    Task<Group> CreateGroup(string groupName, string shortcut, GroupKind groupKind = GroupKind.Regular);
    Task<Group?> GetGroupByName(string groupName);
    Task<Group?> GetGroupByNameOrShortCut(string groupName, string shortcut);
    Task<bool> GroupExistsByNameOrShorCut(string groupName, string shortcut);
    bool IsUserInGroup(Entity entity, int groupId);
    Task<bool> RemoveMember(Entity entity, int groupId);
}

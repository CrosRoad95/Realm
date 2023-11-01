namespace RealmCore.Server.Interfaces;

public interface IGroupService
{
    Task<bool> AddMember(RealmPlayer player, int groupId, int rank = 1, string rankName = "");
    Task<Group> CreateGroup(string groupName, string shortcut, GroupKind groupKind = GroupKind.Regular);
    Task<Group?> GetGroupByName(string groupName);
    Task<Group?> GetGroupByNameOrShortCut(string groupName, string shortcut);
    Task<bool> GroupExistsByNameOrShorCut(string groupName, string shortcut);
    bool IsUserInGroup(RealmPlayer player, int groupId);
    Task<bool> RemoveMember(RealmPlayer player, int groupId);
}

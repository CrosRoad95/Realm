namespace RealmCore.Server.Interfaces;

public interface IGroupService
{
    Task<bool> TryAddMember(RealmPlayer player, int groupId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default);
    Task<Group> CreateGroup(string groupName, string shortcut, GroupKind groupKind = GroupKind.Regular, CancellationToken cancellationToken = default);
    Task<Group?> GetGroupByName(string groupName, CancellationToken cancellationToken = default);
    Task<Group?> GetGroupByNameOrShortCut(string groupName, string shortcut, CancellationToken cancellationToken = default);
    Task<bool> GroupExistsByNameOrShorCut(string groupName, string shortcut, CancellationToken cancellationToken = default);
    Task<bool> RemoveMember(RealmPlayer player, int groupId, CancellationToken cancellationToken = default);
}

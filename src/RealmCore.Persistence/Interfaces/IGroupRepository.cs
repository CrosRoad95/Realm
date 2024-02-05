namespace RealmCore.Persistence.Interfaces;

public interface IGroupRepository
{
    Task<GroupData> Create(string groupName, string shortcut, byte kind = 1, CancellationToken cancellationToken = default);
    Task<GroupMemberData?> TryAddMember(int groupId, int userId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default);
    Task<bool> ExistsByName(string groupName, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameOrShortcut(string groupName, string shortcut, CancellationToken cancellationToken = default);
    Task<bool> ExistsByShortcut(string shortcut, CancellationToken cancellationToken = default);
    Task<GroupData?> GetByName(string groupName, CancellationToken cancellationToken = default);
    Task<GroupData?> GetGroupByNameOrShortcut(string groupName, string shortcut, CancellationToken cancellationToken = default);
    Task<bool> IsUserInGroup(int groupId, int userId, CancellationToken cancellationToken = default);
    Task<bool> TryRemoveMember(int groupId, int userId, CancellationToken cancellationToken = default);
}

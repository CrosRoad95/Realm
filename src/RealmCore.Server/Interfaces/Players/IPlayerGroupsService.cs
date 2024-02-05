
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerGroupsService : IPlayerService
{
    internal bool AddGroupMember(GroupMemberData groupMemberData);
    internal bool RemoveGroupMember(int groupId);
    bool IsMember(int groupId);
    GroupMemberData? GetMemberOrDefault(int groupId);
}

namespace Realm.Server.Exceptions;

public class GroupMemberNotFoundException : Exception
{
    public GroupMemberNotFoundException(int groupId, int userId) : base($"User of id {userId} is not found in group {groupId}.") { }
}

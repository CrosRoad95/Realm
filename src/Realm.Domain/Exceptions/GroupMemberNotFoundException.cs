namespace Realm.Domain.Exceptions;

public class GroupMemberNotFoundException : Exception
{
    public GroupMemberNotFoundException(int groupId, Guid userId) : base($"User of id {userId} is not found in group {groupId}.") { }
}

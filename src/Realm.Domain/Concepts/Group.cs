namespace Realm.Domain.Concepts;

public struct Group
{
    public int id;
    public string name;
    public string? shortcut;
    public GroupKind kind;
    public GroupMember[] members;
}

namespace RealmCore.Server.Structs;

public struct Group
{
    public int id;
    public string name;
    public string? shortcut;
    public GroupKind kind;
    public GroupMember[] members;
}

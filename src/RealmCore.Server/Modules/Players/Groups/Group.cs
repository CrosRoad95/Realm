namespace RealmCore.Server.Modules.Players.Groups;

public struct Group
{
    public int id;
    public string name;
    public string? shortcut;
    public GroupKind kind;
    public GroupMember[] members;
}

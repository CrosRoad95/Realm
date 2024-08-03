namespace RealmCore.Server.Modules.Players.Groups;

public struct Group
{
    public required int id;
    public required string name;
    public required string? shortcut;
    public required int kind;
    public required GroupMember[] members;
}

public struct GroupMember
{
    public required int userId;
    public required int rank;
    public required string rankName;
}

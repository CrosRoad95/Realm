namespace Realm.Persistance.Data;

public sealed class Group
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Shortcut { get; set; }
    public byte? Kind { get; set; }

    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
}

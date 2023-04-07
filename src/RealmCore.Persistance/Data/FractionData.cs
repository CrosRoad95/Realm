namespace RealmCore.Persistance.Data;

public sealed class FractionData
{
    public int Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string Name { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string? Code { get; set; }

    public ICollection<FractionMemberData> Members { get; set; } = new List<FractionMemberData>();
}

namespace Realm.Persistance.Data;

public class Fraction
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Code { get; set; }

    public ICollection<FractionMember> Members { get; set; } = new List<FractionMember>();
}

namespace Realm.Domain.Concepts;

public class ConstructionInfo
{
    public byte Interior { get; set; } = 0;
    public ushort Dimension { get; set; } = 0;
    public string? Id { get; set; } = null;
}

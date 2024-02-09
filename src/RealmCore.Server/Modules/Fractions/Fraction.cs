namespace RealmCore.Server.Modules.Fractions;

public struct Fraction
{
    public int id;
    public string name;
    public string? code;
    public Vector3 position;
    public List<FractionMember> members;
}

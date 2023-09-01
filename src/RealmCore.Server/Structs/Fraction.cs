namespace RealmCore.Server.Structs;

public struct Fraction
{
    public int id;
    public string name;
    public string? code;
    public Vector3 position;
    public List<FractionMember> members;
}

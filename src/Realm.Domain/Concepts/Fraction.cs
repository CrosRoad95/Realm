using Realm.Domain.Enums;

namespace Realm.Domain.Concepts;

public struct Fraction
{
    public int id;
    public string name;
    public string? code;
    public Vector3 position;
    public FractionMember[] members;
}

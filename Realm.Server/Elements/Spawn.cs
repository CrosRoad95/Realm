namespace Realm.Server.Elements;

public class Spawn : Element
{
    private readonly string _id;

    private readonly bool _isPersistant = PersistantScope.IsPersistant;

    public Spawn(string id, string name, Vector3 position, Vector3 rotation)
    {
        Name = name;
        _id = id;
        Position = position;
        Rotation = rotation;
    }

    public bool IsPersistant() => _isPersistant;

    public override string ToString() => "Spawn";
}

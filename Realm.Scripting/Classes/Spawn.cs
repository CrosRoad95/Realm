namespace Realm.Scripting.Classes;

public class Spawn : ISpawn
{
    private readonly string _id;
    private readonly string _name;

    public string Id => _id.ToString();
    public string Name => _name;
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    public bool IsPersistant { get; } = PersistantScope.IsPersistant;

    public Spawn(string id, string name, Vector3 position, Vector3 rotation)
    {
        _id = id;
        _name = name;
        Position = position;
        Rotation = rotation;
    }

    public override string ToString() => "Spawn";
}

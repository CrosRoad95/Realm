namespace Realm.Scripting.Classes;

public class Spawn : ISpawn
{
    private readonly Guid _id;
    private readonly string _name;
    private readonly Vector3 _position;
    private readonly Vector3 _rotation;

    public string Id => _id.ToString();
    public string Name => _name;
    public Vector3 Position => _position;
    public Vector3 Rotation => _rotation;

    public Spawn(Guid id, string name, Vector3 position, Vector3 rotation)
    {
        _id = id;
        _name = name;
        _position = position;
        _rotation = rotation;
    }

    public override string ToString() => "Spawn";
}

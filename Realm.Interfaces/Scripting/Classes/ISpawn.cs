namespace Realm.Interfaces.Scripting.Classes;

public interface ISpawn
{
    string Id { get; }
    string Name { get; }
    Vector3 Position { get; }
    Vector3 Rotation { get; }
}

namespace Realm.Interfaces.Scripting.Classes;

[Name("Spawn")]
public interface ISpawn : IMovable
{
    string Id { get; }
    string Name { get; }
}

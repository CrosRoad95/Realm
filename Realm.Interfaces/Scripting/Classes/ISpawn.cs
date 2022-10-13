namespace Realm.Interfaces.Scripting.Classes;

[Name("Spawn")]
public interface ISpawn : IElement, IMovable
{
    string Name { get; }
}

namespace Realm.Interfaces.Server;

[Name("Player")]
public interface IRPGPlayer : IMovable
{
    string Name { get; set; }

    void Spawn(ISpawn spawn);
}

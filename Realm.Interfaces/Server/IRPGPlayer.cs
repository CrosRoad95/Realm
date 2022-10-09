namespace Realm.Interfaces.Server;

[Name("Player")]
public interface IRPGPlayer
{
    string Name { get; set; }

    void Spawn(ISpawn spawn);
}

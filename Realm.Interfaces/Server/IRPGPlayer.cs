using Realm.Interfaces.Scripting.Classes;

namespace Realm.Interfaces.Server;

public interface IRPGPlayer
{
    string Name { get; set; }

    void Spawn(ISpawn spawn);
}

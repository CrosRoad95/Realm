using Realm.Interfaces.Scripting.Classes;

namespace Realm.Interfaces.Server;

public interface ISpawnManager
{
    ISpawn CreateSpawn(string name, Vector3 position, Vector3 rotation);
}

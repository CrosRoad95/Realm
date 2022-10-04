namespace Realm.Interfaces.Server;

public interface ISpawnManager
{
    Guid CreateSpawn(string name, Vector3 position, Vector3 rotation);
}

namespace Realm.Interfaces.Server;

public interface ISpawnManager
{
    ISpawn CreateSpawn(string id, string name, Vector3 position, Vector3 rotation);
    ISpawn[] GetAll();
}

namespace Realm.Server.Managers;

internal class SpawnManager : ISpawnManager
{
    private readonly List<ISpawn> _spawns = new();
    public SpawnManager() { }

    public ISpawn CreateSpawn(string name, Vector3 position, Vector3 rotation)
    {
        var spawn = new Spawn(Guid.NewGuid(), name, position, rotation);
        _spawns.Add(spawn);
        return spawn;
    }

    public ISpawn[] GetAll() => _spawns.ToArray();
}

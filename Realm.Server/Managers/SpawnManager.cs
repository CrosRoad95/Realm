namespace Realm.Server.Managers;

internal class SpawnManager : ISpawnManager
{
    private readonly Dictionary<string, ISpawn> _spawns = new();
    public SpawnManager() { }

    public ISpawn CreateSpawn(string id, string name, Vector3 position, Vector3 rotation)
    {
        var spawn = new Spawn(id, name, position, rotation);
        _spawns.Add(id, spawn);
        return spawn;
    }

    public IElement[] GetAll() => _spawns.Values.ToArray();

    public IElement? GetById(string id)
    {
        _spawns.TryGetValue(id, out var spawn);
        return spawn;
    }
}

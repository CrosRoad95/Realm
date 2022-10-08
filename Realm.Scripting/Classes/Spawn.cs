namespace Realm.Scripting.Classes;

public class Spawn : ISpawn
{
    private readonly ISpawnManager _spawnManager;
    private readonly Guid _id;

    public Spawn(ISpawnManager spawnManager, Guid id)
    {
        _spawnManager = spawnManager;
        _id = id;
    }

    public string Id => _id.ToString();
}

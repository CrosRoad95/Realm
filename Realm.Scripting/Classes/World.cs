namespace Realm.Scripting.Classes;

public class World : IWorld
{
    private readonly ISpawnManager _spawnManager;
    private readonly IPlayerManager _playerManager;

    public World(ISpawnManager spawnManager, IPlayerManager playerManager)
    {
        _spawnManager = spawnManager;
        _playerManager = playerManager;
    }

    public ISpawn CreateSpawn(string id, string name, Vector3 position, Vector3? rotation = null)
    {
        var spawn = _spawnManager.CreateSpawn(id, name, position, rotation ?? Vector3.Zero);
        return spawn;
    }

    private ICommonManager GetManagerByType(string type)
    {
        return type switch
        {
            "spawn" => _spawnManager,
            "player" => _playerManager,
            _ => throw new ScriptEngineException($"Unsupported element type '{type}'")
        };
    }

    public object GetElementsByType(string type)
    {
        var manager = GetManagerByType(type);
        var elements = manager.GetAll();
        return elements.ToScriptArray();
    }
    
    public IElement? GetElementByTypeAndId(string type, string id)
    {
        var manager = GetManagerByType(type);
        return manager.GetById(id);
    }

    public override string ToString() => "World";
}

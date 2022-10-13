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

    public object GetElementsByType(string type)
    {
        object[] elements;
        switch(type)
        {
            case "spawn":
                elements = _spawnManager.GetAll();
                break;
            case "player":
                elements = _playerManager.GetAll();
                break;
            default:
                throw new ScriptEngineException($"Unsupported element type '{type}'");
        }

        return elements.ToScriptArray();
    }

    public override string ToString() => "World";
}

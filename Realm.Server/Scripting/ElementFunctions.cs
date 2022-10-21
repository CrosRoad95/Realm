namespace Realm.Server.Scripting;

public class ElementFunctions
{
    private readonly RPGServer _rpgServer;
    private readonly IElementCollection _elementCollection;

    public ElementFunctions(RPGServer rpgServer, IElementCollection elementCollection)
    {
        _rpgServer = rpgServer;
        _elementCollection = elementCollection;
    }

    public Spawn CreateSpawn(string id, string name, Vector3 position, Vector3? rotation = null)
    {
        var spawn = new Spawn(id, name, position, rotation ?? Vector3.Zero);
        _rpgServer.AssociateElement(spawn);
        return spawn;
    }

    private IEnumerable<object> GetCollectionByType(string type)
    {
        return type switch
        {
            "spawn" => _elementCollection.GetByType<Spawn>().Cast<object>(),
            "player" => _elementCollection.GetByType<Player>().Cast<object>(),
            _ => throw new NotSupportedException($"Unsupported element type '{type}'")
        };
    }

    public object GetElementsByType(string type)
    {
        var elements = GetCollectionByType(type);
        return elements.ToArray().ToScriptArray();
    }

    public int CountElementsByType(string type)
    {
        var elements = GetCollectionByType(type);
        return elements.Count();
    }
}

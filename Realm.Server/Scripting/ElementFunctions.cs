namespace Realm.Server.Scripting;

public class ElementFunctions
{
    private readonly RPGServer _rpgServer;
    private readonly IElementCollection _elementCollection;
    private readonly AuthorizationPoliciesProvider _authorizationPoliciesProvider;

    public ElementFunctions(RPGServer rpgServer, IElementCollection elementCollection, AuthorizationPoliciesProvider authorizationPoliciesProvider)
    {
        _rpgServer = rpgServer;
        _elementCollection = elementCollection;
        _authorizationPoliciesProvider = authorizationPoliciesProvider;
    }

    public Spawn CreateSpawn(string id, string name, Vector3 position, Vector3? rotation = null)
    {
        var spawn = new Spawn(_authorizationPoliciesProvider, id, name, position, rotation ?? Vector3.Zero);
        _rpgServer.AssociateElement(spawn);
        return spawn;
    }

    [NoScriptAccess]
    public IEnumerable<object> GetCollectionByType(string type)
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

    public bool DestroyElement(Element element)
    {
        switch (element)
        {
            case Player _:
                throw new Exception("Can not destroy persistant element.");
            case Spawn spawn:
                if (spawn.IsPersistant())
                    throw new Exception("Can not destroy persistant element.");
                break;
        }

        if (IsElement(element))
        {
            _elementCollection.Remove(element);
            return true;
        }

        return false;
    }

    public bool IsElement(Element element)
    {
        if (_elementCollection.Get(element.Id) != null)
            return true;
        return _elementCollection.GetAll().Any(x => x == element);
    }
}

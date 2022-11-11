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
        var spawn = _rpgServer.GetRequiredService<Spawn>();
        spawn.AssignId(id);
        spawn.Name = name;
        spawn.Position = position;
        if (rotation != null)
            spawn.Rotation = rotation ?? Vector3.Zero;
        _rpgServer.AssociateElement(spawn);
        return spawn;
    }

    public RPGVehicle CreateVehicle(string id, string name, ushort model, Vector3 position, Vector3? rotation = null)
    {
        var vehicle = _rpgServer.GetRequiredService<RPGVehicle>();
        vehicle.AssignId(id);
        vehicle.Name = name;
        vehicle.Model = model;
        vehicle.Position = position;
        if(rotation != null)
            vehicle.Rotation = rotation ?? Vector3.Zero;
        _rpgServer.AssociateElement(vehicle);
        return vehicle;
    }

    [NoScriptAccess]
    public IEnumerable<object> GetCollectionByType(string type)
    {
        return type switch
        {
            "spawn" => _elementCollection.GetByType<Spawn>().Cast<object>(),
            "player" => _elementCollection.GetByType<Player>().Cast<object>(),
            "vehicle" => _elementCollection.GetByType<RPGVehicle>().Cast<object>(),
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
            case RPGVehicle vehicle:
                if (vehicle.IsPersistant())
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

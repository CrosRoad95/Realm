using Realm.Server.Elements.Variants;
using SlipeServer.Server.Elements.IdGeneration;

namespace Realm.Server.Scripting;

public class ElementFunctions
{
    private readonly RPGServer _rpgServer;
    private readonly IElementCollection _elementCollection;
    private readonly IElementIdGenerator _elementIdGenerator;

    public ElementFunctions(RPGServer rpgServer, IElementCollection elementCollection, IElementIdGenerator elementIdGenerator)
    {
        _rpgServer = rpgServer;
        _elementCollection = elementCollection;
        _elementIdGenerator = elementIdGenerator;
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

    public RPGVehicle CreateVehicle(ushort model, Vector3 position, Vector3? rotation = null)
    {
        var vehicle = _rpgServer.GetRequiredService<RPGVehicle>();
        vehicle.Model = model;
        vehicle.Position = position;
        if(rotation != null)
            vehicle.Rotation = rotation ?? Vector3.Zero;
        _rpgServer.AssociateElement(vehicle);
        return vehicle;
    }
    
    public RPGBlip CreateBlip(int icon, Vector3 position)
    {
        if (!Enum.IsDefined(typeof(BlipIcon), icon))
            throw new Exception("Invalid icon");

        var blip = _rpgServer.GetRequiredService<RPGBlip>();
        blip.Icon = (BlipIcon)icon;
        blip.Position = position;
        _rpgServer.AssociateElement(blip);
        return blip;
    }

    public RPGVariantBlip CreateVariantBlip(Vector3 position)
    {
        var blip = _rpgServer.GetRequiredService<RPGBlip>();
        blip.Id = _elementIdGenerator.GetId();
        blip.SetIsVariant();
        blip.Position = position;
        return new RPGVariantBlip(blip);
    }

    public RPGRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color)
    {
        var radarArea = _rpgServer.GetRequiredService<RPGRadarArea>();
        radarArea.Position2 = position;
        radarArea.Size = size;
        radarArea.Color = color;
        _rpgServer.AssociateElement(radarArea);
        return radarArea;
    }
    
    public RPGVariantRadarArea CreateVariantRadarArea(Vector2 position, Vector2 size)
    {
        var variant = _rpgServer.GetRequiredService<RPGRadarArea>();
        variant.Id = _elementIdGenerator.GetId();
        variant.SetIsVariant();
        variant.Position2 = position;
        variant.Size = size;
        return new RPGVariantRadarArea(variant);
    }

    public RPGFraction CreateFraction(string code, string name, Vector3 position)
    {
        var fraction = _rpgServer.GetRequiredService<RPGFraction>();
        fraction.Code = code;
        fraction.Name = name;
        fraction.Position = position;
        return fraction;
    }

    [NoScriptAccess]
    public IEnumerable<object> GetCollectionByType(string type)
    {
        return type switch
        {
            "spawn" => _elementCollection.GetByType<Spawn>().Cast<object>(),
            "player" => _elementCollection.GetByType<Player>().Cast<object>(),
            "vehicle" => _elementCollection.GetByType<RPGVehicle>().Cast<object>(),
            "blip" => _elementCollection.GetByType<RPGBlip>().Cast<object>(),
            "radararea" => _elementCollection.GetByType<RPGRadarArea>().Cast<object>(),
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

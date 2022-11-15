using Realm.Server.Elements.Variants;
using SlipeServer.Server.Elements.IdGeneration;

namespace Realm.Server.Scripting;

[NoDefaultScriptAccess]
public class ElementScriptingFunctions
{
    private readonly RPGServer _rpgServer;
    private readonly IElementCollection _elementCollection;
    private readonly IElementIdGenerator _elementIdGenerator;

    public ElementScriptingFunctions(RPGServer rpgServer, IElementCollection elementCollection, IElementIdGenerator elementIdGenerator)
    {
        _rpgServer = rpgServer;
        _elementCollection = elementCollection;
        _elementIdGenerator = elementIdGenerator;
    }

    [ScriptMember("createSpawn")]
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

    [ScriptMember("createVehicle")]
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

    [ScriptMember("createBlip")]
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

    [ScriptMember("createVariantBlip")]
    public RPGVariantBlip CreateVariantBlip(Vector3 position)
    {
        var blip = _rpgServer.GetRequiredService<RPGBlip>();
        blip.Id = _elementIdGenerator.GetId();
        blip.SetIsVariant();
        blip.Position = position;
        return new RPGVariantBlip(blip);
    }

    [ScriptMember("createRadarArea")]
    public RPGRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color)
    {
        var radarArea = _rpgServer.GetRequiredService<RPGRadarArea>();
        radarArea.Position2 = position;
        radarArea.Size = size;
        radarArea.Color = color;
        _rpgServer.AssociateElement(radarArea);
        return radarArea;
    }

    [ScriptMember("createVariantRadarArea")]
    public RPGVariantRadarArea CreateVariantRadarArea(Vector2 position, Vector2 size)
    {
        var variant = _rpgServer.GetRequiredService<RPGRadarArea>();
        variant.Id = _elementIdGenerator.GetId();
        variant.SetIsVariant();
        variant.Position2 = position;
        variant.Size = size;
        return new RPGVariantRadarArea(variant);
    }

    [ScriptMember("createFraction")]
    public RPGFraction CreateFraction(string code, string name, Vector3 position)
    {
        var fraction = _rpgServer.GetRequiredService<RPGFraction>();
        fraction.Code = code;
        fraction.Name = name;
        fraction.Position = position;
        return fraction;
    }

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

    [ScriptMember("getElementsByType")]
    public object GetElementsByType(string type)
    {
        var elements = GetCollectionByType(type);
        return elements.ToArray().ToScriptArray();
    }

    [ScriptMember("countElementsByType")]
    public int CountElementsByType(string type)
    {
        var elements = GetCollectionByType(type);
        return elements.Count();
    }

    [ScriptMember("destroyElement")]
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
            if(element is IDisposable)
                ((IDisposable)element).Dispose();
            return true;
        }

        return false;
    }

    [ScriptMember("isElement")]
    public bool IsElement(Element element)
    {
        if (_elementCollection.Get(element.Id) != null)
            return true;
        return _elementCollection.GetAll().Any(x => x == element);
    }
}

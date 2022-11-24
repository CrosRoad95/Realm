using Realm.Persistance.Services;
using Realm.Server.ElementCollections;
using Realm.Server.Elements.CollisionShapes;
using Realm.Server.Elements.Variants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.IdGeneration;
using PersistantVehicleData = Realm.Persistance.Data.Vehicle;

namespace Realm.Server.Scripting;

[NoDefaultScriptAccess]
public class ElementScriptingFunctions
{
    private readonly RPGServer _rpgServer;
    private readonly IElementCollection _elementCollection;
    private readonly IElementIdGenerator _elementIdGenerator;
    private readonly ElementByStringIdCollection _elementByStringIdCollection;
    private readonly PeriodicEntitySaveService _periodicEntitySaveService;
    private readonly IDb _db;
    private readonly ILogger _logger;

    public ElementScriptingFunctions(RPGServer rpgServer, IElementCollection elementCollection, IElementIdGenerator elementIdGenerator, ElementByStringIdCollection elementByStringIdCollection,
        PeriodicEntitySaveService periodicEntitySaveService, IDb db, ILogger logger)
    {
        _rpgServer = rpgServer;
        _elementCollection = elementCollection;
        _elementIdGenerator = elementIdGenerator;
        _elementByStringIdCollection = elementByStringIdCollection;
        _periodicEntitySaveService = periodicEntitySaveService;
        _db = db;
        _logger = logger.ForContext<ElementScriptingFunctions>();
    }

    [ScriptMember("setElementId")]
    public bool SetElementId(Element element, string id)
    {
        return _elementByStringIdCollection.AssignElementToId(element, id);
    }

    [ScriptMember("getElementId")]
    public string? GetElementId(Element element)
    {
        return _elementByStringIdCollection.GetElementId(element);
    }

    [ScriptMember("getElementById")]
    public Element? GetElementById(string id)
    {
        return _elementByStringIdCollection.GetElementById(id);
    }
    
    [ScriptMember("createSpawn")]
    public RPGSpawn CreateSpawn(Vector3 position, Vector3? rotation = null)
    {
        var spawn = _rpgServer.GetRequiredService<RPGSpawn>();
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

    [ScriptMember("createPersistantVehicle")]
    public async Task<bool> CreatePersistantVehicle(string id, ushort model, Vector3 position, Vector3? rotation = null)
    {
        using var _ = new PersistantScope();
        if (await _db.Vehicles.AnyAsync(x => x.Id == id))
            return false;

        var vehicleData = new PersistantVehicleData
        {
            Id = id,
            Model = model,
            Platetext = "",
            TransformAndMotion = new Persistance.Data.Helpers.TransformAndMotion
            {
                Position = position,
                Rotation = rotation ?? Vector3.Zero
            },
            CreatedAt = DateTime.Now,
        };
        _db.Vehicles.Add(vehicleData);
        await _db.SaveChangesAsync();
        _logger.Verbose("Created persistant vehicle {vehicleId}", id);
        return true;
    }

    [ScriptMember("spawnPersistantVehicle")]
    public async Task<RPGVehicle?> SpawnPersistantVehicle(string id, Vector3? position = null, Vector3? rotation = null)
    {
        using var _ = new PersistantScope();
        var vehicle = _rpgServer.GetRequiredService<RPGVehicle>();
        vehicle.AssignId(id);
        var loaded = await vehicle.Load();
        if(!loaded)
        {
            vehicle.Dispose();
            throw new Exception("Failed to create vehicle");
        }
        if (position != null)
            vehicle.Position = position ?? Vector3.Zero;
        if (rotation != null)
            vehicle.Rotation = rotation ?? Vector3.Zero;
        _periodicEntitySaveService.VehicleCreated(vehicle);
        _rpgServer.AssociateElement(vehicle);
        _logger.Verbose("Spawned persistant vehicle {vehicleId}", id);
        return vehicle;
    }

    [ScriptMember("createBlip")]
    public RPGBlip CreateBlip(Vector3 position, int icon)
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
    

    [ScriptMember("createPickup")]
    public RPGPickup CreatePickup(Vector3 position, ushort model)
    {
        var pickup = _rpgServer.GetRequiredService<RPGPickup>();
        pickup.Position = position;
        pickup.Model = model;
        _rpgServer.AssociateElement(pickup);
        _rpgServer.AssociateElement(pickup.CollisionShape);
        return pickup;
    }

    [ScriptMember("createVariantRadarArea")]
    public RPGVariantPickup CreateVariantRadarArea(Vector3 position, ushort model)
    {
        var variant = _rpgServer.GetRequiredService<RPGPickup>();
        variant.Id = _elementIdGenerator.GetId();
        variant.SetIsVariant();
        variant.Position = position;
        variant.Model = model;
        return new RPGVariantPickup(variant);
    }

    [ScriptMember("createFraction")]
    public RPGFraction CreateFraction(string code, string name, Vector3 position)
    {
        var fraction = _rpgServer.GetRequiredService<RPGFraction>();
        fraction.Code = code;
        fraction.Name = name;
        fraction.Position = position;
        _rpgServer.AssociateElement(fraction);
        return fraction;
    }
    

    [ScriptMember("createColSphere")]
    public RPGCollisionSphere CreateColSphere(Vector3 position, float radius)
    {
        var collisionSphere = _rpgServer.GetRequiredService<RPGCollisionSphere>();
        collisionSphere.Position = position;
        collisionSphere.Radius = radius;
        collisionSphere.Position = position;
        return collisionSphere;
    }

    public IEnumerable<object> GetCollectionByType(string type)
    {
        return type switch
        {
            "spawn" => _elementCollection.GetAll().Where(x => x is RPGSpawn).Cast<object>(),
            "player" => _elementCollection.GetByType<Player>().Cast<object>(),
            "vehicle" => _elementCollection.GetByType<RPGVehicle>().Cast<object>(),
            "blip" => _elementCollection.GetByType<RPGBlip>().Cast<object>(),
            "radararea" => _elementCollection.GetByType<RPGRadarArea>().Cast<object>(),
            "pickup" => _elementCollection.GetByType<RPGPickup>().Cast<object>(),
            "fraction" => _elementCollection.GetAll().Where(x => x is RPGFraction).Cast<object>(),
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
        bool wasDestroyed = false;
        switch (element)
        {
            case Player _:
                throw new Exception("Can not destroy persistant element.");
            case RPGSpawn spawn:
                if (spawn.IsPersistant())
                    throw new Exception("Can not destroy persistant element.");
                wasDestroyed = true;
                break;
            case RPGVehicle vehicle:
                vehicle.Destroy();
                if(vehicle.IsPersistant())
                    _logger.Verbose("Destroyed persistant vehicle of id: {vehicleId}", vehicle.VehicleId);
                wasDestroyed = true;
                break;
        }

        if (wasDestroyed && IsElement(element))
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

using Realm.Persistance.Services;
using Realm.Server.Elements.CollisionShapes;
using Realm.Server.Elements.Variants;
using SlipeServer.Server.Elements.IdGeneration;
using PersistantVehicleData = Realm.Persistance.Data.Vehicle;

namespace Realm.Server.Factories;

public class RPGElementsFactory
{
    private readonly RPGServer _rpgServer;
    private readonly IDb _db;
    private readonly ILogger _logger;
    private readonly PeriodicEntitySaveService _periodicEntitySaveService;
    private readonly IElementIdGenerator _elementIdGenerator;

    public event Action<RPGSpawn>? SpawnCreated;
    public event Action<RPGVehicle>? VehicleCreated;
    public event Action<RPGBlip>? BlipCreated;

    public RPGElementsFactory(RPGServer rpgServer, IDb db, ILogger logger, PeriodicEntitySaveService periodicEntitySaveService,
        IElementIdGenerator elementIdGenerator)
    {
        _rpgServer = rpgServer;
        _db = db;
        _logger = logger.ForContext<RPGElementsFactory>();
        _periodicEntitySaveService = periodicEntitySaveService;
        _elementIdGenerator = elementIdGenerator;
    }

    public RPGSpawn CreateSpawn(Vector3 position, Vector3? rotation = null)
    {
        var spawn = _rpgServer.GetRequiredService<RPGSpawn>();
        spawn.Position = position;
        if (rotation != null)
            spawn.Rotation = rotation ?? Vector3.Zero;
        _rpgServer.AssociateElement(spawn);
        SpawnCreated?.Invoke(spawn);
        return spawn;
    }

    public RPGVehicle CreateVehicle(ushort model, Vector3 position, Vector3? rotation = null)
    {
        var vehicle = _rpgServer.GetRequiredService<RPGVehicle>();
        vehicle.Model = model;
        vehicle.Position = position;
        if (rotation != null)
            vehicle.Rotation = rotation ?? Vector3.Zero;
        _rpgServer.AssociateElement(vehicle);
        VehicleCreated?.Invoke(vehicle);
        return vehicle;
    }

    public async Task<RPGVehicle?> CreateNewPersistantVehicle(string id, ushort model, Vector3 position, Vector3? rotation = null)
    {
        using var _ = new PersistantScope();
        if (await _db.Vehicles.AnyAsync(x => x.Id == id))
            return null;

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
        _logger.Verbose("Created new persistant vehicle {vehicleId}", id);
        return await SpawnPersistantVehicle(id, position, rotation);
    }

    public async Task<RPGVehicle?> SpawnPersistantVehicle(string id, Vector3? position = null, Vector3? rotation = null)
    {
        using var _ = new PersistantScope();
        var vehicle = _rpgServer.GetRequiredService<RPGVehicle>();
        vehicle.AssignId(id);
        var loaded = await vehicle.Load();
        if (!loaded)
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
        VehicleCreated?.Invoke(vehicle);
        return vehicle;
    }

    public RPGBlip CreateBlip(Vector3 position, int icon)
    {
        if (!Enum.IsDefined(typeof(BlipIcon), icon))
            throw new Exception("Invalid icon");

        var blip = _rpgServer.GetRequiredService<RPGBlip>();
        blip.Icon = (BlipIcon)icon;
        blip.Position = position;
        _rpgServer.AssociateElement(blip);
        BlipCreated?.Invoke(blip);
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

    public RPGPickup CreatePickup(Vector3 position, ushort model)
    {
        var pickup = _rpgServer.GetRequiredService<RPGPickup>();
        pickup.Position = position;
        pickup.Model = model;
        _rpgServer.AssociateElement(pickup);
        _rpgServer.AssociateElement(pickup.CollisionShape);
        return pickup;
    }

    public RPGVariantPickup CreateVariantRadarArea(Vector3 position, ushort model)
    {
        var variant = _rpgServer.GetRequiredService<RPGPickup>();
        variant.Id = _elementIdGenerator.GetId();
        variant.SetIsVariant();
        variant.Position = position;
        variant.Model = model;
        return new RPGVariantPickup(variant);
    }

    public RPGFraction CreateFraction(string code, string name, Vector3 position)
    {
        var fraction = _rpgServer.GetRequiredService<RPGFraction>();
        fraction.Code = code;
        fraction.Name = name;
        fraction.Position = position;
        _rpgServer.AssociateElement(fraction);
        return fraction;
    }

    public RPGCollisionSphere CreateColSphere(Vector3 position, float radius)
    {
        var collisionSphere = _rpgServer.GetRequiredService<RPGCollisionSphere>();
        collisionSphere.Position = position;
        collisionSphere.Radius = radius;
        collisionSphere.Position = position;
        return collisionSphere;
    }
}

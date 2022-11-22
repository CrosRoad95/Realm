using Vehicle = SlipeServer.Server.Elements.Vehicle;
using PersistantVehicleData = Realm.Persistance.Data.Vehicle;
using Realm.Server.Components;

namespace Realm.Server.Elements;

[NoDefaultScriptAccess]
public class RPGVehicle : Vehicle, IPersistantVehicle, IDisposable
{
    private bool _disposed = false;
    private string? _vehicleId = null;
    private readonly ILogger _logger;
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly IDb _db;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;
    public string VehicleId { get => _vehicleId ?? throw new InvalidDataException(nameof(Id)); }

    public event Action<IPersistantVehicle>? NotifyNotSavedState;
    public event Action<IPersistantVehicle>? Disposed;
    private PersistantVehicleData? _vehicleData;

    [ScriptMember("components")]
    public ComponentSystem Components;

    [ScriptMember("isFrozen")]
    public bool IsFrozen
    {
        get => base.IsFrozen;
        set
        {
            base.IsFrozen = value;
            if(_vehicleData != null)
            {
                _vehicleData.IsFrozen = value;
                NotifyNotSavedState?.Invoke(this);
            }
        }
    }

    public RPGVehicle(ILogger logger, EventScriptingFunctions eventFunctions, IDb db) : base(404, new Vector3(0,0, 10000))
    {
        _eventFunctions = eventFunctions;
        _db = db;
        _logger = logger
            .ForContext<RPGVehicle>()
            .ForContext(new RPGVehicleEnricher(this));
        IsFrozen = true;
        Components = new ComponentSystem(this, _logger);
        PedLeft += RPGVehicle_PedLeft;
    }

    private void RPGVehicle_PedLeft(Element sender, VehicleLeftEventArgs e)
    {
        if(e.Seat == 0)
            NotifyNotSavedState?.Invoke(this);
    }

    public void AssignId(string id)
    {
        _vehicleId = id;
    }

    [ScriptMember("spawn")]
    public async Task<bool> Spawn(Spawn spawn)
    {
        Position = spawn.Position;
        Rotation = spawn.Rotation;
        using var vehicleSpawnedEvent = new VehicleSpawnedEvent(this, spawn);
        await _eventFunctions.InvokeEvent(vehicleSpawnedEvent);
        _logger.Verbose("Spawned at {spawn}", spawn);
        return true;
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    [ScriptMember("isPersistant")]
    public bool IsPersistant()
    {
        CheckIfDisposed();
        return _isPersistant;
    }

    public async Task<bool> Load()
    {
        _vehicleData = await _db.Vehicles.Include(x => x.VehicleData)
            .Where(x => x.Id == VehicleId)
            .FirstOrDefaultAsync();
        if(_vehicleData != null)
        {
            Position = _vehicleData.TransformAndMotion.Position;
            Rotation = _vehicleData.TransformAndMotion.Rotation;
            if(!string.IsNullOrEmpty(_vehicleData.Components))
            {
                Components = JsonConvert.DeserializeObject<ComponentSystem>(_vehicleData.Components, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                }) ?? throw new JsonSerializationException("Failed to deserialize Components");
                Components.SetLogger(_logger);
                Components.SetOwner(this);
                Components.AfterLoad();
            }
        }
        return _vehicleData != null;
    }

    public async Task Save()
    {
        if (_vehicleData == null)
            return;
        _vehicleData.Components = JsonConvert.SerializeObject(Components, Formatting.None, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
        });
        _db.Vehicles.Update(_vehicleData);
        await _db.SaveChangesAsync();
    }

    public string LongUserFriendlyName() => Name;
    public override string ToString() => Name;

    public void Dispose()
    {
        Disposed?.Invoke(this);
        _disposed = true;
    }
}

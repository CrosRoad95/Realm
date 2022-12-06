using Vehicle = SlipeServer.Server.Elements.Vehicle;
using PersistantVehicleData = Realm.Persistance.Data.Vehicle;
using VehicleUpgrade = Realm.Server.Concepts.Upgrades.VehicleUpgrade;
using SlipeServer.Server.Constants;

namespace Realm.Server.Elements;

[NoDefaultScriptAccess]
public class RPGVehicle : Vehicle, IPersistantVehicle, IWorldDebugData, IDisposable
{
    private bool _disposed = false;
    private string? _vehicleId = null;
    private readonly ILogger _logger;
    private readonly IDb _db;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;

    public event Action<RPGVehicle, RPGSpawn?>? Spawned;

    private readonly Guid _debugId = Guid.NewGuid();
    [ScriptMember("debugId")]
    public Guid DebugId => _debugId;
    public PreviewType PreviewType => PreviewType.None;
    public Color PreviewColor => Color.FromArgb(100, 200, 0, 0);

    public string VehicleId { get => _vehicleId ?? throw new InvalidDataException(nameof(Id)); }

    public event Action<IPersistantVehicle>? NotifyNotSavedState;
    public event Action<IPersistantVehicle>? Disposed;
    private PersistantVehicleData? _vehicleData;

    [ScriptMember("components")]
    public ComponentSystem Components;

    [ScriptMember("isFrozen")]
    public new bool IsFrozen
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

    private readonly List<VehicleUpgrade> _upgrades = new();

    public RPGVehicle(ILogger logger, IDb db) : base(404, new Vector3(0,0, 10000))
    {
        _db = db;
        _logger = logger
            .ForContext<RPGVehicle>()
            .ForContext(new RPGVehicleEnricher(this));
        IsFrozen = true;
        Components = new ComponentSystem();
        Components.SetOwner(this);
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
    public bool Spawn(RPGSpawn spawn)
    {
        Position = spawn.Position;
        Rotation = spawn.Rotation;
        Spawned?.Invoke(this, spawn);
        _logger.Verbose("{player} spawned at {spawn}", this, spawn);
        return true;
    }

    [ScriptMember("addUpgrade")]
    public bool AddUpgrade(VehicleUpgrade upgrade, bool rebuild = true)
    {
        _upgrades.Add(upgrade);
        if(rebuild)
            RebuildUpgrades();
        return true;
    }

    private void RebuildUpgrades()
    {
        var handling = VehicleHandlingConstants.DefaultVehicleHandling[model];
        foreach (var upgrade in _upgrades.Select(x => x.MaxVelocity).Where(x => x != null))
        {
            handling.MaxVelocity += upgrade.IncreaseByUnits;
            handling.MaxVelocity *= upgrade.MultipleBy;
        }
        foreach (var upgrade in _upgrades.Select(x => x.EngineAcceleration).Where(x => x != null))
        {
            handling.EngineAcceleration += upgrade.IncreaseByUnits;
            handling.EngineAcceleration *= upgrade.MultipleBy;
        }

        Handling = handling;
    }
    
    public bool RawSpawn(Vector3 position, Vector3 rotation)
    {
        Position = position;
        Rotation = rotation;
        Spawned?.Invoke(this, null);
        _logger.Verbose("{vehicle} spawned at position: {position}, rotation: {rotation}", this, position, rotation);
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
            if(!string.IsNullOrEmpty(_vehicleData.Components))
            {
                Components = ComponentSystem.CreateFromString(_vehicleData.Components);
                Components.SetOwner(this);
            }
            RawSpawn(_vehicleData.TransformAndMotion.Position, _vehicleData.TransformAndMotion.Rotation);
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

    [ScriptMember("toString")]
    public override string ToString() => Name;

    public void Dispose()
    {
        Disposed?.Invoke(this);
        _disposed = true;
    }
}

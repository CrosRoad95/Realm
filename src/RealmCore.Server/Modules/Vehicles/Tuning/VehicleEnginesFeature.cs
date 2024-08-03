namespace RealmCore.Server.Modules.Vehicles.Tuning;

public sealed class VehicleEnginesFeature : IVehicleFeature, IUsesVehiclePersistentData
{
    private readonly object _lock = new();
    private ICollection<VehicleEngineData> _vehicleEngine = [];

    public event Action<VehicleEnginesFeature, short>? ActiveEngineChanged;
    public event Action<VehicleEnginesFeature, short>? EngineAdded;
    public event Action<VehicleEnginesFeature, short>? EngineRemoved;
    public event Action? VersionIncreased;

    public short ActiveEngineId
    {
        get
        {
            lock (_lock)
                return _vehicleEngine.First(x => x.Selected).EngineId;
        }
        set
        {
            lock (_lock)
            {
                foreach (var vehicleEngine in _vehicleEngine)
                {
                    vehicleEngine.Selected = vehicleEngine.EngineId == value;
                }
            }
            ActiveEngineChanged?.Invoke(this, value);
        }
    }

    public short[] EnginesIds
    {
        get
        {
            lock (_lock)
            {
                return [.. _vehicleEngine.Select(x => x.EngineId)];
            }
        }
    }

    public RealmVehicle Vehicle { get; }

    public VehicleEnginesFeature(VehicleContext vehicleContext)
    {
        Vehicle = vehicleContext.Vehicle;
    }

    private bool InternalHasEngine(short engineId)
    {
        return _vehicleEngine.Any(x => x.EngineId == engineId);
    }

    public bool Has(short engineId)
    {
        lock (_lock)
            return InternalHasEngine(engineId);
    }

    public bool TryAdd(short engineId)
    {
        lock (_lock)
        {
            if (InternalHasEngine(engineId))
                return false;

            _vehicleEngine.Add(new VehicleEngineData
            {
                EngineId = engineId
            });
        }
        EngineAdded?.Invoke(this, engineId);
        VersionIncreased?.Invoke();
        return true;
    }

    public bool TryRemove(short engineId)
    {
        lock (_lock)
        {
            if (!InternalHasEngine(engineId))
                return false;
            var engine = _vehicleEngine.First(x => x.EngineId == engineId);
            _vehicleEngine.Remove(engine);
        }
        EngineRemoved?.Invoke(this, engineId);
        VersionIncreased?.Invoke();
        return true;
    }

    public void Loaded(VehicleData vehicleData, bool preserveData = false)
    {
        _vehicleEngine = vehicleData.VehicleEngines;
    }

    public void Unloaded()
    {
    }
}

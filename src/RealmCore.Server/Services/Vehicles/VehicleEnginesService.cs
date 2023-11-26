namespace RealmCore.Server.Services.Vehicles;

public sealed class VehicleEnginesService : IVehicleEnginesService
{
    private ICollection<VehicleEngineData> _vehicleEngine = [];
    private object _lock = new();

    public event Action<IVehicleEnginesService, short>? ActiveEngineChanged;
    public event Action<IVehicleEnginesService, short>? EngineAdded;
    public event Action<IVehicleEnginesService, short>? EngineRemoved;

    public short ActiveEngineId
    {
        get
        {
            lock(_lock)
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

    public List<short> EnginesIds
    {
        get
        {
            lock (_lock)
            {
                return new List<short>(_vehicleEngine.Select(x => x.EngineId));
            }
        }
    }

    public RealmVehicle Vehicle { get; }
    public VehicleEnginesService(VehicleContext vehicleContext, IVehiclePersistanceService vehiclePersistanceService)
    {
        Vehicle = vehicleContext.Vehicle;
        vehiclePersistanceService.Loaded += HandleLoaded;
    }

    private void HandleLoaded(IVehiclePersistanceService persistance, RealmVehicle vehicle)
    {
        _vehicleEngine = persistance.VehicleData.VehicleEngines;
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

    public void Add(short engineId)
    {
        lock (_lock)
        {
            if(InternalHasEngine(engineId))
                throw new Exception($"Vehicle engine id '{engineId}' already added");
            _vehicleEngine.Add(new VehicleEngineData
            {
                EngineId = engineId
            });
        }
        EngineAdded?.Invoke(this, engineId);
    }

    public void Remove(short engineId)
    {
        lock (_lock)
        {
            if (!InternalHasEngine(engineId))
                throw new Exception($"Vehicle engine id '{engineId}' doesn't exists");
            var engine = _vehicleEngine.First(x => x.EngineId == engineId);
            _vehicleEngine.Remove(engine);
        }
        EngineRemoved?.Invoke(this, engineId);
    }
}

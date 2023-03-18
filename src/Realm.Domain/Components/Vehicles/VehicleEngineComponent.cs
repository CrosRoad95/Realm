namespace Realm.Domain.Components.Vehicles;

[ComponentUsage(true)]
public sealed class VehicleEngineComponent : Component
{
    private List<int> _vehicleEngineIds = new();
    private int _activeVehicleEngineId;
    private object _lock = new();

    public event Action<VehicleEngineComponent, int>? ActiveVehicleEngineChanged;
    public event Action<VehicleEngineComponent, int>? VehicleEngineAdded;
    public event Action<VehicleEngineComponent, int>? VehicleEngineRemoved;

    public int ActiveVehicleEngineId { get => _activeVehicleEngineId; set
        {
            lock(_lock)
            {
                if(!_vehicleEngineIds.Contains(value))
                    throw new Exception($"Vehicle engine id '{value}' is not valid id");

                _activeVehicleEngineId = value;
            }
            ActiveVehicleEngineChanged?.Invoke(this, value);
        }
    }

    public List<int> VehicleEngineIds
    {
        get
        {
            lock(_lock)
            {
                return new List<int>(_vehicleEngineIds);
            }
        }
    }

    internal int UpgradeId => Entity.GetRequiredService<VehicleEnginesRegistry>().Get(ActiveVehicleEngineId).UpgradeId;

    internal VehicleEngineComponent(ICollection<VehicleEngine> vehicleEngines)
    {
        _activeVehicleEngineId = vehicleEngines.First(x => x.Selected).EngineId;
        _vehicleEngineIds = vehicleEngines.Select(x => (int)x.EngineId).ToList();
        if(!_vehicleEngineIds.Any())
        {
            throw new Exception("No vehicle engines loaded");
        }
    }

    public VehicleEngineComponent() : this(new int[] { 1 }) { }

    public VehicleEngineComponent(IEnumerable<int> vehicleEngineIds, int activeVehicleEngineId = 1)
    {
        if (!vehicleEngineIds.Any())
            throw new Exception("No vehicle engine ids provided");
        if (!vehicleEngineIds.Contains(activeVehicleEngineId))
            throw new Exception($"Vehicle engine id '{activeVehicleEngineId}' is not valid id");
        _vehicleEngineIds = vehicleEngineIds.ToList();
        _activeVehicleEngineId = activeVehicleEngineId;
    }

    public void AddEngine(int engineId)
    {
        lock(_lock)
        {
            if(_vehicleEngineIds.Contains(engineId))
            {
                throw new Exception($"Vehicle engine id '{engineId}' already added");
            }
            _vehicleEngineIds.Add(engineId);
        }
        VehicleEngineAdded?.Invoke(this, engineId);
    }

    public void RemoveEngine(int engineId)
    {
        lock (_lock)
        {
            if(_activeVehicleEngineId == engineId)
            {
                throw new Exception($"Can not remove active engine");
            }
            _vehicleEngineIds.Remove(engineId);
        }
        VehicleEngineAdded?.Invoke(this, engineId);
    }
}

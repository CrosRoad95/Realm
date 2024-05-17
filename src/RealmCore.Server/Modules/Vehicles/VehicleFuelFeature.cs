namespace RealmCore.Server.Modules.Vehicles;

public interface IVehicleFuelFeature : IVehicleFeature, IEnumerable<FuelContainer>
{
    FuelContainer? Active { get; }

    event Action<IVehicleFuelFeature, FuelContainer?>? ActiveChanged;

    FuelContainer AddFuelContainer(short fuelType, float initialAmount, float maxCapacity, float fuelConsumptionPerOneKm, float minimumDistanceThreshold, bool makeActive = false);
    void Update(bool forceUpdate = false);
}

internal sealed class VehicleFuelFeature : IVehicleFuelFeature, IUsesVehiclePersistentData
{
    private readonly object _lock = new();
    private ICollection<VehicleFuelData> _vehicleFuelData = [];
    private readonly List<FuelContainer> _fuelContainers = [];
    private FuelContainer? _active;

    public RealmVehicle Vehicle { get; init; }
    public event Action<IVehicleFuelFeature, FuelContainer?>? ActiveChanged;
    public event Action? VersionIncreased;

    public FuelContainer? Active
    {
        get => _active; private set
        {
            lock (_lock)
            {
                if (value != Active)
                {
                    foreach (var item in _fuelContainers)
                    {
                        if (item != value)
                            item.Active = false;
                    }
                    _active = value;
                    ActiveChanged?.Invoke(this, _active);
                }
            }
        }
    }

    public VehicleFuelFeature(VehicleContext vehicleContext)
    {
        Vehicle = vehicleContext.Vehicle;
        Vehicle.PositionChanged += HandlePositionChanged;
    }

    private void HandlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        if (args.IsSync)
        {
            Update();
        }
    }

    public void Update(bool forceUpdate = false)
    {
        var active = Active;
        if(active != null)
            active.Update(forceUpdate);
    }

    public FuelContainer AddFuelContainer(short fuelType, float initialAmount, float maxCapacity, float fuelConsumptionPerOneKm, float minimumDistanceThreshold, bool makeActive = false)
    {
        var fuelData = new VehicleFuelData
        {
            FuelType = fuelType,
            Amount = initialAmount,
            MaxCapacity = maxCapacity,
            FuelConsumptionPerOneKm = fuelConsumptionPerOneKm,
            MinimumDistanceThreshold = minimumDistanceThreshold,
            Active = makeActive
        };
        var fuelContainer = new FuelContainer(Vehicle, fuelData);

        lock (_lock)
        {
            _vehicleFuelData.Add(fuelData);
            _fuelContainers.Add(fuelContainer);
        }

        if (makeActive)
            Active = fuelContainer;
        return fuelContainer;
    }

    public IEnumerator<FuelContainer> GetEnumerator()
    {
        lock (_lock)
            return new List<FuelContainer>(_fuelContainers).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Loaded(VehicleData vehicleData)
    {
        _vehicleFuelData = vehicleData.Fuels;
        FuelContainer? active = null;
        lock (_lock)
        {
            foreach (var fuelData in vehicleData.Fuels)
            {
                var fuelContainer = new FuelContainer(Vehicle, fuelData);
                if (fuelData.Active && active == null)
                {
                    active = fuelContainer;
                }
                _fuelContainers.Add(fuelContainer);
            }
        }

        if (active != null)
        {
            Active = active;
            Update(true);
        }
    }

    public void Unloaded()
    {

    }
}

namespace RealmCore.Server.Modules.Vehicles;

public interface IVehicleFuelFeature : IVehicleFeature, IEnumerable<FuelContainer>
{
    FuelContainer AddFuelContainer(short fuelType, float initialAmount, float maxCapacity, float fuelConsumptionPerOneKm, float minimumDistanceThreshold, bool makeActive = false);
    void Update(bool forceUpdate = false);
}

internal sealed class VehicleFuelFeature : IVehicleFuelFeature
{
    private readonly object _lock = new();
    private ICollection<VehicleFuelData> _vehicleFuelData = [];
    private readonly List<FuelContainer> _fuelContainers = [];

    public RealmVehicle Vehicle { get; init; }

    public VehicleFuelFeature(VehicleContext vehicleContext, IVehiclePersistenceFeature persistanceService)
    {
        Vehicle = vehicleContext.Vehicle;
        persistanceService.Loaded += HandleLoaded;
        Vehicle.PositionChanged += HandlePositionChanged;
    }

    private void HandlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        if (args.IsSync)
        {
            Update();
        }
    }

    private void InternalUpdate(bool forceUpdate = false)
    {
        foreach (var item in _fuelContainers)
        {
            item.Update(forceUpdate);
        }
    }

    public void Update(bool forceUpdate = false)
    {
        lock (_lock)
        {
            InternalUpdate(forceUpdate);
        }
    }

    public FuelContainer AddFuelContainer(short fuelType, float initialAmount, float maxCapacity, float fuelConsumptionPerOneKm, float minimumDistanceThreshold, bool makeActive = false)
    {
        lock (_lock)
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
            _vehicleFuelData.Add(fuelData);
            _fuelContainers.Add(fuelContainer);
            return fuelContainer;
        }
    }

    private void HandleLoaded(IVehiclePersistenceFeature persistance, RealmVehicle vehicle)
    {
        _vehicleFuelData = persistance.VehicleData.Fuels;
        lock (_lock)
        {
            foreach (var fuelData in persistance.VehicleData.Fuels)
            {
                _fuelContainers.Add(new FuelContainer(Vehicle, fuelData));
            }
            InternalUpdate(true);
        }
    }

    public IEnumerator<FuelContainer> GetEnumerator()
    {
        lock (_lock)
            return new List<FuelContainer>(_fuelContainers).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

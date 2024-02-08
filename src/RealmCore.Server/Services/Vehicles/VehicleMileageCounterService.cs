namespace RealmCore.Server.Services.Vehicles;

public interface IVehicleMileageCounterService : IVehicleService
{
    float Mileage { get; set; }
    float MinimumDistanceThreshold { get; set; }

    event Action<IVehicleMileageCounterService, float, float>? Traveled;
}

internal sealed class VehicleMileageCounterService : IVehicleMileageCounterService, IDisposable
{
    private Vector3 _lastPosition;
    private float _mileage;
    private float _minimumDistanceThreshold = 2.0f;
    private readonly IUpdateService _updateService;

    public event Action<IVehicleMileageCounterService, float, float>? Traveled;

    public float Mileage
    {
        get
        {
            return _mileage;
        }
        set
        {
            if (value < 0.0f) value = 0.0f;
            _mileage = value;
        }
    }

    public float MinimumDistanceThreshold
    {
        get
        {
            return _minimumDistanceThreshold;
        }
        set
        {
            if (value < 0.0f) value = 0.0f;
            _minimumDistanceThreshold = value;
        }
    }

    public RealmVehicle Vehicle { get; init; }

    public VehicleMileageCounterService(VehicleContext vehicleContext, IVehiclePersistanceService persistance, IUpdateService updateService)
    {
        _updateService = updateService;
        Vehicle = vehicleContext.Vehicle;
        persistance.Loaded += HandleLoaded;
        _updateService.RareUpdate += HandleRareUpdate;
    }

    private void HandleLoaded(IVehiclePersistanceService persistance, RealmVehicle vehicle)
    {
        _mileage = persistance.VehicleData.Mileage;
    }

    public void HandleRareUpdate()
    {
        if (Vehicle == null)
            return;

        if (!Vehicle.IsEngineOn)
        {
            _lastPosition = Vehicle.Position;
            return;
        }
        if (!Vehicle.IsEngineOn || Vehicle.IsFrozen)
            return;

        var traveledDistance = Vehicle.Position - _lastPosition;
        var traveledDistanceNumber = traveledDistance.Length();
        if (_minimumDistanceThreshold > traveledDistanceNumber)
            return;
        _lastPosition = Vehicle.Position;
        _mileage += traveledDistanceNumber;
        Traveled?.Invoke(this, _mileage, traveledDistanceNumber);
    }

    public void Dispose()
    {
        _updateService.RareUpdate -= HandleRareUpdate;
    }
}

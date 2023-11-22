namespace RealmCore.Server.Services.Vehicles;

internal class VehicleMileageService : IVehicleMileageService
{
    public RealmVehicle Vehicle { get; }

    private Vector3 _lastPosition;
    private float _mileage;
    private float _minimumDistanceThreshold = 2.0f;
    private RealmVehicle? _vehicle;

    public event Action<IVehicleMileageService, float, float>? Traveled;

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

    public VehicleMileageService(VehicleContext vehicleContext, IVehiclePersistanceService persistance)
    {
        Vehicle = vehicleContext.Vehicle;
        persistance.Loaded += HandleLoaded;
    }

    private void HandleLoaded(IVehiclePersistanceService persistance, RealmVehicle vehicle)
    {
        _mileage = persistance.VehicleData.Mileage;
    }

    public void RareUpdate()
    {
        if (_vehicle == null)
            return;

        if (!_vehicle.IsEngineOn)
        {
            _lastPosition = _vehicle.Position;
            return;
        }
        if (!_vehicle.IsEngineOn || _vehicle.IsFrozen)
            return;

        var traveledDistance = _vehicle.Position - _lastPosition;
        var traveledDistanceNumber = traveledDistance.Length();
        if (_minimumDistanceThreshold > traveledDistanceNumber)
            return;
        _lastPosition = _vehicle.Position;
        _mileage += traveledDistanceNumber;
        Traveled?.Invoke(this, _mileage, traveledDistanceNumber);
    }
}

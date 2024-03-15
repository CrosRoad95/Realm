using RealmCore.Server.Modules.Vehicles.Persistence;

namespace RealmCore.Server.Modules.Vehicles;

public interface IVehicleMileageCounterFeature : IVehicleFeature
{
    float Mileage { get; set; }
    float MinimumDistanceThreshold { get; set; }

    event Action<IVehicleMileageCounterFeature, float, float>? Traveled;
}

internal sealed class VehicleMileageCounterFeature : IVehicleMileageCounterFeature, IUsesVehiclePersistentData, IDisposable
{
    private Vector3 _lastPosition;
    private float _mileage;
    private float _minimumDistanceThreshold = 2.0f;

    public event Action<IVehicleMileageCounterFeature, float, float>? Traveled;
    public event Action? VersionIncreased;

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

    public VehicleMileageCounterFeature(VehicleContext vehicleContext)
    {
        Vehicle = vehicleContext.Vehicle;
        Vehicle.PositionChanged += HandlePositionChanged;
    }

    private void HandlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        var traveledDistance = Vehicle.Position - _lastPosition;
        var traveledDistanceNumber = traveledDistance.Length();
        if (_minimumDistanceThreshold > traveledDistanceNumber)
            return;
        _lastPosition = Vehicle.Position;
        HandleUpdate(traveledDistanceNumber);
    }

    public void HandleUpdate(float traveledDistance)
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

        _mileage += traveledDistance;
        Traveled?.Invoke(this, _mileage, traveledDistance);
    }

    public void Dispose()
    {
        Vehicle.PositionChanged -= HandlePositionChanged;
    }

    public void Loaded(VehicleData vehicleData)
    {
        _mileage = vehicleData.Mileage;
    }

    public void Unloaded()
    {
    }
}

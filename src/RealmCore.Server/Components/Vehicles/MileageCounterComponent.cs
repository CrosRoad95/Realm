namespace RealmCore.Server.Components.Vehicles;

public class MileageCounterComponent : Component, IRareUpdateCallback
{
    private Vector3 _lastPosition;
    private float _mileage;
    private float _minimumDistanceThreshold;
    private VehicleElementComponent? _vehicleElementComponent;

    public event Action<MileageCounterComponent, float, float>? Traveled;

    public float Mileage
    {
        get
        {
            ThrowIfDisposed();
            return _mileage;
        }
        set
        {
            ThrowIfDisposed();
            if (value < 0.0f) value = 0.0f;
            _mileage = value;
        }
    }

    public float MinimumDistanceThreshold
    {
        get
        {
            ThrowIfDisposed();
            return _minimumDistanceThreshold;
        }
        set
        {
            ThrowIfDisposed();
            if (value < 0.0f) value = 0.0f;
            _minimumDistanceThreshold = value;
        }
    }

    public MileageCounterComponent()
    {
        _mileage = 0.0f;
        _minimumDistanceThreshold = 2.0f;
    }

    public MileageCounterComponent(float mileage, float minimumDistanceThreshold = 2.0f)
    {
        _mileage = mileage;
        _minimumDistanceThreshold = minimumDistanceThreshold;
    }

    protected override void Attach()
    {
        _vehicleElementComponent = Entity.GetRequiredComponent<VehicleElementComponent>();
    }

    public void RareUpdate()
    {
        if (_vehicleElementComponent == null)
            return;

        var transform = Entity.Transform;
        if (!_vehicleElementComponent.IsEngineOn)
        {
            _lastPosition = transform.Position;
            return;
        }
        if (!_vehicleElementComponent.IsEngineOn || _vehicleElementComponent.IsFrozen)
            return;

        var traveledDistance = transform.Position - _lastPosition;
        var traveledDistanceNumber = traveledDistance.Length();
        if (_minimumDistanceThreshold > traveledDistanceNumber)
            return;
        _lastPosition = transform.Position;
        _mileage += traveledDistanceNumber;
        Traveled?.Invoke(this, _mileage, traveledDistanceNumber);
    }
}

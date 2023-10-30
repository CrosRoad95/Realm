namespace RealmCore.Server.Components.Vehicles;

public class MileageCounterComponent : ComponentLifecycle, IRareUpdateCallback
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

    public override void Attach()
    {
        _vehicleElementComponent = Entity.GetRequiredComponent<VehicleElementComponent>();
    }

    public void RareUpdate()
    {
        if (_vehicleElementComponent == null)
            return;

        if (!_vehicleElementComponent.IsEngineOn)
        {
            _lastPosition = _vehicleElementComponent.Position;
            return;
        }
        if (!_vehicleElementComponent.IsEngineOn || _vehicleElementComponent.IsFrozen)
            return;

        var traveledDistance = _vehicleElementComponent.Position - _lastPosition;
        var traveledDistanceNumber = traveledDistance.Length();
        if (_minimumDistanceThreshold > traveledDistanceNumber)
            return;
        _lastPosition = _vehicleElementComponent.Position;
        _mileage += traveledDistanceNumber;
        Traveled?.Invoke(this, _mileage, traveledDistanceNumber);
    }
}

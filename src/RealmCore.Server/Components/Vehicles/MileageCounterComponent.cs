namespace RealmCore.Server.Components.Vehicles;

public class MileageCounterComponent : ComponentLifecycle, IRareUpdateCallback
{
    private Vector3 _lastPosition;
    private float _mileage;
    private float _minimumDistanceThreshold;
    private RealmVehicle? _vehicle;

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
        _vehicle = (RealmVehicle)Element;
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

namespace Realm.Domain.Components.Vehicles;

public class MileageCounterComponent : Component
{
    private Vector3 _lastPosition;
    private float _mileage;
    private float _minimumDistanceThreshold;

    public float Mileage
    {
        get => _mileage;
        set
        {
            if (value < 0.0f) value = 0.0f;
            _mileage = value;
        }
    }

    public float MinimumDistanceThreshold
    {
        get => _minimumDistanceThreshold;
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

    public override Task Load()
    {
        Entity.Transform.PositionChanged += HandlePositionChanged;
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        base.Dispose();
        Entity.Transform.PositionChanged -= HandlePositionChanged;
    }

    private void HandlePositionChanged(Transform transform)
    {
        Update();
    }

    private void Update()
    {
        var vehicle = Entity.GetRequiredComponent<VehicleElementComponent>();
        if (!vehicle.IsEngineOn)
        {
            _lastPosition = Entity.Transform.Position;
            return;
        }
        if (!vehicle.IsEngineOn || vehicle.IsFrozen)
            return;

        var traveledDistance = Entity.Transform.Position - _lastPosition;
        if (_minimumDistanceThreshold > traveledDistance.Length())
            return;
        _lastPosition = Entity.Transform.Position;
        _mileage += traveledDistance.Length();
    }
}

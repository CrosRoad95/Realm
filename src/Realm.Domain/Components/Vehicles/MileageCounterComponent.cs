using Realm.Domain.Components.Elements;
using SlipeServer.Server.Elements.Events;

namespace Realm.Domain.Components.Vehicles;

[Serializable]
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

    public override Task Load()
    {
        var vehicle = Entity.InternalGetRequiredComponent<VehicleElementComponent>().Vehicle;
        vehicle.PositionChanged += HandlePositionChanged;
        return Task.CompletedTask;
    }

    private void HandlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        if (args.IsSync)
            Update();
    }

    private void Update()
    {
        var vehicle = Entity.InternalGetRequiredComponent<VehicleElementComponent>().Vehicle;
        if (!vehicle.IsEngineOn)
        {
            _lastPosition = vehicle.Position;
            return;
        }
        if (!vehicle.IsEngineOn || vehicle.IsFrozen)
            return;

        var traveledDistance = vehicle.Position - _lastPosition;
        if (_minimumDistanceThreshold > traveledDistance.Length())
            return;
        _lastPosition = vehicle.Position;
        _mileage += traveledDistance.Length();
    }
}

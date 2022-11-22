using System.Runtime.Serialization;

namespace Realm.Server.Components;

[Serializable]
public class MileageCounterComponent : IElementComponent
{
    private RPGVehicle _rpgVehicle = default!;
    private ILogger _logger = default!;

    private Vector3 _lastPosition;
    private float _mileage;
    private float _minimumDistanceThreshold;

    [ScriptMember("name")]
    public string Name => "MileageCounter";

    [ScriptMember("mileage")]
    public float Mileage
    {
        get => _mileage;
        set
        {
            if (value < 0.0f) value = 0.0f;
            _mileage = value;
        }
    }

    [ScriptMember("minimumDistanceThreshold")]
    public float MinimumDistanceThreshold
    {
        get => _minimumDistanceThreshold;
        set
        {
            if (value < 0.0f) value = 0.0f;
            _minimumDistanceThreshold = value;
        }
    }

    public MileageCounterComponent(double minimumDistanceThreshold, double initialMileage = 0)
    {
        if (initialMileage < 0) throw new ArgumentOutOfRangeException(nameof(initialMileage));

        _mileage = (float)initialMileage;
        _minimumDistanceThreshold = (float)minimumDistanceThreshold;
    }

    public MileageCounterComponent(SerializationInfo info, StreamingContext context)
    {
        _mileage = (float?)info.GetValue("Mileage", typeof(float)) ?? throw new SerializationException();
        _minimumDistanceThreshold = (float?)info.GetValue("MinimumDistanceThreshold", typeof(float)) ?? throw new SerializationException();
    }

    [NoScriptAccess]
    public void SetOwner(Element element)
    {
        if (_rpgVehicle != null)
            throw new Exception("Component already attached to element");
        if (element is not RPGVehicle rpgVehicle)
            throw new Exception("Not supported element type, expected: RPGVehicle");
        _rpgVehicle = rpgVehicle;
        _rpgVehicle.Disposed += Disposed;
        _lastPosition = _rpgVehicle.Position;
        RegisterEvents();
    }

    [NoScriptAccess]
    public void SetLogger(ILogger logger)
    {
        _logger = logger.ForContext<VehicleFuelComponent>();
    }

    private void Disposed(IPersistantVehicle obj)
    {
        UnregisterEvents();
        _rpgVehicle.Disposed -= Disposed;
    }

    private void RegisterEvents()
    {
        _rpgVehicle.PositionChanged += PositionChanged;
    }

    private void UnregisterEvents()
    {
        _rpgVehicle.PositionChanged -= PositionChanged;
    }

    private void PositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        if (args.IsSync)
            Update();
    }

    private void Update()
    {
        if (!_rpgVehicle.IsEngineOn)
        {
            _lastPosition = _rpgVehicle.Position;
            return;
        }
        if ((!_rpgVehicle.IsEngineOn || _rpgVehicle.IsFrozen))
            return;

        var traveledDistance = _rpgVehicle.Position - _lastPosition;
        if (_minimumDistanceThreshold > traveledDistance.Length())
            return;
        _lastPosition = _rpgVehicle.Position;
        _mileage += traveledDistance.Length();
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Mileage", Mileage);
        info.AddValue("MinimumDistanceThreshold", MinimumDistanceThreshold);
    }
}

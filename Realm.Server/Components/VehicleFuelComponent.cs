namespace Realm.Server.Components;

public class VehicleFuelComponent : IComponent
{
    private RPGVehicle _rpgVehicle = default!;
    private ILogger _logger = default!;

    private Vector3 _lastPosition;
    private float _value = 0;
    private float _maxCapacity = 0;
    private float _fuelConsumptionPerOneKm;
    private float _minimumDistanceThresholdSquared;

    [ScriptMember("name")]
    public string Name => "FuelComponent";

    [ScriptMember("minimumDistanceThresholdSquared")]
    public float MinimumDistanceThresholdSquared
    {
        get => _minimumDistanceThresholdSquared; set
        {
            if (value < 0.0f) value = 0.0f;
            _minimumDistanceThresholdSquared = value;
        }
    }
    
    [ScriptMember("fuelConsumptionPer1Km")]
    public float FuelConsumptionPer1Km
    {
        get => _fuelConsumptionPerOneKm; set
        {
            if (value < 0.0f) value = 0.0f;
            _fuelConsumptionPerOneKm = value;
        }
    }

    [ScriptMember("value")]
    public float Value
    {
        get => _value; set
        {
            if (value < 0.0f) value = 0.0f;
            if (value >= MaxCapacity) value = MaxCapacity;
            _value = value;
            Update(true);
        }
    }

    [ScriptMember("maxCapacity")]
    public float MaxCapacity
    {
        get => _maxCapacity; set
        {
            if (value < 0.0f)
                _maxCapacity = 0;
            else
                _maxCapacity = value;
            if (_maxCapacity < _value)
                _value = _maxCapacity;
            Update(true);
        }
    }

    public VehicleFuelComponent(double initialValue, double maxCapacity, double fuelConsumptionPerOneKm, double minimumDistanceThreshold)
    {
        if (initialValue < 0) throw new ArgumentOutOfRangeException(nameof(initialValue));
        if (minimumDistanceThreshold < 0) throw new ArgumentOutOfRangeException(nameof(minimumDistanceThreshold));
        if (fuelConsumptionPerOneKm < 0) throw new ArgumentOutOfRangeException(nameof(fuelConsumptionPerOneKm));
        if (initialValue >= maxCapacity) initialValue = maxCapacity;

        _value = (float)initialValue;
        _maxCapacity = (float)maxCapacity;
        _fuelConsumptionPerOneKm = (float)fuelConsumptionPerOneKm;
        _minimumDistanceThresholdSquared = (float)minimumDistanceThreshold * (float)minimumDistanceThreshold;
    }

    [NoScriptAccess]
    public void SetOwner(Element element)
    {
        if (_rpgVehicle != null)
            throw new Exception("Component already attached to element");
        _rpgVehicle = (RPGVehicle)element;
        _rpgVehicle.Disposed += Disposed;
        _lastPosition = _rpgVehicle.Position;
        RegisterEvents();
        Update(true);
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

    private void Update(bool forceUpdate = false)
    {
        if (!_rpgVehicle.IsEngineOn && !forceUpdate)
        {
            _lastPosition = _rpgVehicle.Position;
            return;
        }
        if ((!_rpgVehicle.IsEngineOn || _rpgVehicle.IsFrozen) && !forceUpdate)
            return;

        var traveledDistance = _rpgVehicle.Position - _lastPosition;
        if (_minimumDistanceThresholdSquared > traveledDistance.LengthSquared() && !forceUpdate)
            return;
        _lastPosition = _rpgVehicle.Position;
        var consumedFuel = (_fuelConsumptionPerOneKm / 1000.0f) * traveledDistance.Length();
        _value = _value - consumedFuel;
        if(_value <= 0)
        {
            _value = 0;
            _rpgVehicle.IsEngineOn = false;
        }
    }
}

using System.Runtime.Serialization;

namespace Realm.Server.Components;

[Serializable]
public class VehicleFuelComponent : IElementComponent
{
    private RPGVehicle _rpgVehicle = default!;
    private ILogger _logger = default!;

    private Vector3 _lastPosition;
    private float _amount = 0;
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
    
    [ScriptMember("FuelConsumptionPerOneKm")]
    public float FuelConsumptionPerOneKm
    {
        get => _fuelConsumptionPerOneKm; set
        {
            if (value < 0.0f) value = 0.0f;
            _fuelConsumptionPerOneKm = value;
        }
    }

    [ScriptMember("value")]
    public float Amount
    {
        get => _amount; set
        {
            if (value < 0.0f) value = 0.0f;
            if (value >= MaxCapacity) value = MaxCapacity;
            _amount = value;
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
            if (_maxCapacity < _amount)
                _amount = _maxCapacity;
            Update(true);
        }
    }

    public VehicleFuelComponent(double initialAmount, double maxCapacity, double fuelConsumptionPerOneKm, double minimumDistanceThreshold)
    {
        if (initialAmount < 0) throw new ArgumentOutOfRangeException(nameof(initialAmount));
        if (minimumDistanceThreshold < 0) throw new ArgumentOutOfRangeException(nameof(minimumDistanceThreshold));
        if (fuelConsumptionPerOneKm < 0) throw new ArgumentOutOfRangeException(nameof(fuelConsumptionPerOneKm));
        if (initialAmount >= maxCapacity) initialAmount = maxCapacity;

        _amount = (float)initialAmount;
        _maxCapacity = (float)maxCapacity;
        _fuelConsumptionPerOneKm = (float)fuelConsumptionPerOneKm;
        _minimumDistanceThresholdSquared = (float)minimumDistanceThreshold * (float)minimumDistanceThreshold;
    }

    public VehicleFuelComponent(SerializationInfo info, StreamingContext context)
    {
        _amount = (float?)info.GetValue("Amount", typeof(float)) ?? throw new SerializationException();
        _maxCapacity = (float?)info.GetValue("MaxCapacity", typeof(float)) ?? throw new SerializationException();
        _fuelConsumptionPerOneKm = (float?)info.GetValue("FuelConsumptionPerOneKm", typeof(float)) ?? throw new SerializationException();
        _minimumDistanceThresholdSquared = (float?)info.GetValue("MinimumDistanceThresholdSquared", typeof(float)) ?? throw new SerializationException();
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
        _amount -= consumedFuel;
        if(_amount <= 0)
        {
            _amount = 0;
            _rpgVehicle.IsEngineOn = false;
            _logger.Verbose("Fuel ran out, turning off engine.");
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Amount", Amount);
        info.AddValue("MaxCapacity", MaxCapacity);
        info.AddValue("FuelConsumptionPerOneKm", FuelConsumptionPerOneKm);
        info.AddValue("MinimumDistanceThresholdSquared", MinimumDistanceThresholdSquared);
    }
}

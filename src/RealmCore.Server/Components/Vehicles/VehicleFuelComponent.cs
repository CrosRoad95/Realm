namespace RealmCore.Server.Components.Vehicles;

public class VehicleFuelComponent : Component, ILuaDebugDataProvider
{
    public event Action<VehicleFuelComponent>? FuelRanOut;

    private Vector3 _lastPosition;
    private float _amount = 0;
    private float _maxCapacity = 0;
    private float _fuelConsumptionPerOneKm;
    private float _minimumDistanceThreshold;
    private short _fuelType;
    private bool _active;

    public bool Active
    {
        get
        {
            ThrowIfDisposed();
            return _active;
        }
        set
        {
            ThrowIfDisposed();
            if (value)
                Update(true);

            _active = value;
        }
    }

    public short FuelType
    {
        get
        {
            ThrowIfDisposed();
            return _fuelType;
        }
        set {
            ThrowIfDisposed();
            _fuelType = value;
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

    public float FuelConsumptionPerOneKm
    {
        get
        {
            ThrowIfDisposed();
            return _fuelConsumptionPerOneKm;
        }
        set
        {
            ThrowIfDisposed();
            if (value < 0.0f) value = 0.0f;
            _fuelConsumptionPerOneKm = value;
        }
    }

    public float Amount
    {
        get
        {
            ThrowIfDisposed();
            return _amount;
        }
        set
        {
            ThrowIfDisposed();
            if (value < 0.0f)
                value = 0.0f;
            if (value >= MaxCapacity)
                value = MaxCapacity;
            _amount = value;
            Update(true);
        }
    }

    public float MaxCapacity
    {
        get
        {
            ThrowIfDisposed();
            return _maxCapacity;
        }

        set
        {
            ThrowIfDisposed();
            if (value < 0.0f)
                _maxCapacity = 0;
            else
                _maxCapacity = value;
            if (_maxCapacity < _amount)
                _amount = _maxCapacity;
            Update(true);
        }
    }

    public VehicleFuelComponent(short fuelType, double initialAmount, double maxCapacity, double fuelConsumptionPerOneKm, double minimumDistanceThreshold)
    {
        if (initialAmount < 0) throw new ArgumentOutOfRangeException(nameof(initialAmount));
        if (minimumDistanceThreshold < 0) throw new ArgumentOutOfRangeException(nameof(minimumDistanceThreshold));
        if (fuelConsumptionPerOneKm < 0) throw new ArgumentOutOfRangeException(nameof(fuelConsumptionPerOneKm));
        if (initialAmount >= maxCapacity) initialAmount = maxCapacity;

        _amount = (float)initialAmount;
        _maxCapacity = (float)maxCapacity;
        _fuelConsumptionPerOneKm = (float)fuelConsumptionPerOneKm;
        _minimumDistanceThreshold = (float)minimumDistanceThreshold;
        _fuelType = fuelType;
    }

    protected override void Load()
    {
        var vehicle = Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle;
        vehicle.PositionChanged += HandlePositionChanged;
        if (_active)
        {
            // Turn off other fuel components if this is active.
            foreach (var item in Entity.Components.OfType<VehicleFuelComponent>().Where(x => x != this))
                item.Active = false;
        }
    }

    protected override void Detached()
    {
        var vehicle = Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle;
        vehicle.PositionChanged -= HandlePositionChanged;
    }

    private void HandlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        if (args.IsSync)
            Update();
    }

    private void Update(bool forceUpdate = false)
    {
        var vehicle = Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle;
        if (!vehicle.IsEngineOn && !forceUpdate)
        {
            _lastPosition = vehicle.Position;
            return;
        }
        if ((!vehicle.IsEngineOn || vehicle.IsFrozen) && !forceUpdate)
            return;

        var traveledDistance = vehicle.Position - _lastPosition;
        if (_minimumDistanceThreshold > traveledDistance.Length() && !forceUpdate)
            return;
        _lastPosition = vehicle.Position;
        var consumedFuel = _fuelConsumptionPerOneKm / 1000.0f * traveledDistance.Length();
        _amount -= consumedFuel;
        if (_amount <= 0)
        {
            _amount = 0;
            vehicle.IsEngineOn = false;
            FuelRanOut?.Invoke(this);
        }
    }

    public LuaValue GetLuaDebugData()
    {
        return new LuaValue(new Dictionary<LuaValue, LuaValue>
        {
            ["Paliwo"] = $"{_amount:0.00}/{_maxCapacity}"
        });
    }
}

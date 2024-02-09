namespace RealmCore.Server.Modules.Vehicles;

public class FuelContainer
{
    public event Action<FuelContainer>? FuelRanOut;

    private Vector3 _lastPosition;
    private float _amount = 0;
    private float _maxCapacity = 0;
    private float _fuelConsumptionPerOneKm;
    private float _minimumDistanceThreshold;
    private short _fuelType;
    private bool _active;
    private readonly VehicleFuelData? _vehicleFuelData;

    public event Action<FuelContainer, bool>? ActiveChanged;
    public event Action<FuelContainer, short>? FuelTypeChanged;
    public event Action<FuelContainer, float>? MinimumDistanceThresholdChanged;
    public event Action<FuelContainer, float>? AmountChanged;
    public event Action<FuelContainer, float>? MaxCapacityChanged;
    public event Action<FuelContainer, float>? FuelConsumptionPerOneKmChanged;

    public bool Active
    {
        get
        {
            return _active;
        }
        set
        {
            if (value)
                Update(true);

            _active = value;
            if (_vehicleFuelData != null)
                _vehicleFuelData.Active = value;
            ActiveChanged?.Invoke(this, value);
        }
    }

    public short FuelType
    {
        get
        {
            return _fuelType;
        }
        set
        {
            _fuelType = value;
            if (_vehicleFuelData != null)
                _vehicleFuelData.FuelType = value;
            FuelTypeChanged?.Invoke(this, value);
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
            if (_vehicleFuelData != null)
                _vehicleFuelData.MinimumDistanceThreshold = value;
            MinimumDistanceThresholdChanged?.Invoke(this, value);
        }
    }

    public float FuelConsumptionPerOneKm
    {
        get
        {
            return _fuelConsumptionPerOneKm;
        }
        set
        {
            if (value < 0.0f) value = 0.0f;
            _fuelConsumptionPerOneKm = value;
            if (_vehicleFuelData != null)
                _vehicleFuelData.FuelConsumptionPerOneKm = value;
            FuelConsumptionPerOneKmChanged?.Invoke(this, value);
        }
    }

    public float Amount
    {
        get
        {
            return _amount;
        }
        set
        {
            if (value < 0.0f)
                value = 0.0f;
            if (value >= MaxCapacity)
                value = MaxCapacity;
            _amount = value;
            if (_vehicleFuelData != null)
                _vehicleFuelData.Amount = value;
            Update(true);
            AmountChanged?.Invoke(this, value);
        }
    }

    public float MaxCapacity
    {
        get
        {
            return _maxCapacity;
        }

        set
        {
            if (value < 0.0f)
                _maxCapacity = 0;
            else
                _maxCapacity = value;
            if (_maxCapacity < _amount)
                _amount = _maxCapacity;
            if (_vehicleFuelData != null)
                _vehicleFuelData.MaxCapacity = value;
            Update(true);
            MaxCapacityChanged?.Invoke(this, value);
        }
    }

    public RealmVehicle Vehicle { get; }

    internal FuelContainer(RealmVehicle vehicle, short fuelType, double initialAmount, double maxCapacity, double fuelConsumptionPerOneKm, double minimumDistanceThreshold)
    {
        if (initialAmount < 0) throw new ArgumentOutOfRangeException(nameof(initialAmount));
        if (minimumDistanceThreshold < 0) throw new ArgumentOutOfRangeException(nameof(minimumDistanceThreshold));
        if (fuelConsumptionPerOneKm < 0) throw new ArgumentOutOfRangeException(nameof(fuelConsumptionPerOneKm));
        if (initialAmount >= maxCapacity) initialAmount = maxCapacity;

        _amount = (float)initialAmount;
        _maxCapacity = (float)maxCapacity;
        _fuelConsumptionPerOneKm = (float)fuelConsumptionPerOneKm;
        _minimumDistanceThreshold = (float)minimumDistanceThreshold;
        Vehicle = vehicle;
        _fuelType = fuelType;
    }

    internal FuelContainer(RealmVehicle vehicle, VehicleFuelData vehicleFuelData) : this(vehicle, vehicleFuelData.FuelType, vehicleFuelData.Amount, vehicleFuelData.MaxCapacity, vehicleFuelData.FuelConsumptionPerOneKm, 2)
    {
        Vehicle = vehicle;
        _vehicleFuelData = vehicleFuelData;
    }

    public void Update(bool forceUpdate = false)
    {
        if (!Vehicle.IsEngineOn && !forceUpdate)
        {
            _lastPosition = Vehicle.Position;
            return;
        }
        if ((!Vehicle.IsEngineOn || Vehicle.IsFrozen) && !forceUpdate)
            return;

        var traveledDistance = Vehicle.Position - _lastPosition;
        if (_minimumDistanceThreshold > traveledDistance.Length() && !forceUpdate)
            return;
        _lastPosition = Vehicle.Position;
        var consumedFuel = _fuelConsumptionPerOneKm / 1000.0f * traveledDistance.Length();

        _amount -= consumedFuel;
        if (_amount <= 0)
        {
            _amount = 0;
            Vehicle.IsEngineOn = false;
            FuelRanOut?.Invoke(this);
        }
        AmountChanged?.Invoke(this, _amount);
    }
}

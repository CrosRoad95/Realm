namespace RealmCore.Server.Modules.Vehicles;

public interface IVehiclePartDamageFeature : IVehicleFeature
{
    short[] Parts { get; }

    /// <summary>
    /// Triggered when part damage drop to zero.
    /// </summary>
    event Action<IVehiclePartDamageFeature, short>? PartDestroyed;

    bool TryAddPart(short partId, float state);
    float GetState(short partId);
    bool HasPart(short partId);
    void Modify(short partId, float difference);
    bool TryRemovePart(short partId);
    bool TryGetState(short partId, out float state);
}

internal sealed class VehiclePartDamageFeature : IVehiclePartDamageFeature, IUsesVehiclePersistentData
{
    private readonly object _lock = new();
    private ICollection<VehiclePartDamageData> _vehiclePartDamages = [];
    public event Action<IVehiclePartDamageFeature, short>? PartDestroyed;
    public event Action? VersionIncreased;

    public short[] Parts
    {
        get
        {
            lock (_lock)
            {
                return [.. _vehiclePartDamages.Select(x => x.PartId)];
            }
        }
    }

    public RealmVehicle Vehicle { get; init; }

    public VehiclePartDamageFeature(VehicleContext vehicleContext)
    {
        Vehicle = vehicleContext.Vehicle;
    }

    public bool TryAddPart(short partId, float state)
    {
        lock (_lock)
        {
            if (state < 0)
                throw new ArgumentOutOfRangeException(nameof(state));
            var exists = _vehiclePartDamages.FirstOrDefault(x => x.PartId == partId);
            if (exists != null)
                return false;

            _vehiclePartDamages.Add(new VehiclePartDamageData
            {
                PartId = partId,
                State = state
            });
        }

        if (state == 0)
            PartDestroyed?.Invoke(this, partId);

        return true;
    }

    public bool TryRemovePart(short partId)
    {
        bool removed = false;
        lock (_lock)
        {
            var vehiclePartDamage = _vehiclePartDamages.FirstOrDefault(x => x.PartId == partId) ?? throw new ArgumentException("Part id doesn't exists");
            _vehiclePartDamages.Remove(vehiclePartDamage);
        }
        PartDestroyed?.Invoke(this, partId);

        return removed;
    }

    public void Modify(short partId, float difference)
    {
        float newState = 0;
        lock (_lock)
        {
            var vehiclePartDamage = _vehiclePartDamages.FirstOrDefault(x => x.PartId == partId) ?? throw new ArgumentException("Part id doesn't exists");
            vehiclePartDamage.State += difference;
            if (vehiclePartDamage.State <= 0)
                vehiclePartDamage.State = 0;
            newState = vehiclePartDamage.State;
        }

        if (newState == 0)
            PartDestroyed?.Invoke(this, partId);
    }

    public bool HasPart(short partId)
    {
        lock (_lock)
        {
            return _vehiclePartDamages.Any(x => x.PartId == partId);
        }
    }

    public bool TryGetState(short partId, out float state)
    {
        lock (_lock)
        {
            var vehiclePartDamage = _vehiclePartDamages.FirstOrDefault(x => x.PartId == partId);
            if (vehiclePartDamage == null)
            {
                state = 0;
                return false;
            }
            state = vehiclePartDamage.State;
            return true;
        }
    }

    public float GetState(short partId)
    {
        lock (_lock)
        {
            var vehiclePartDamage = _vehiclePartDamages.First(x => x.PartId == partId);
            return vehiclePartDamage.State;
        }
    }

    public void Loaded(VehicleData vehicleData, bool preserveData = false)
    {
        _vehiclePartDamages = vehicleData.PartDamages;
    }

    public void Unloaded()
    {

    }
}

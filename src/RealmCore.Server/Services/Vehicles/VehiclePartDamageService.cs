namespace RealmCore.Server.Services.Vehicles;

public class VehiclePartDamageService : IVehiclePartDamageService
{
    private object _lock = new();
    private ICollection<VehiclePartDamageData> _vehiclePartDamages = [];
    /// <summary>
    /// Triggered when part damage drop to zero.
    /// </summary>
    public event Action<IVehiclePartDamageService, short>? PartDestroyed;

    public IReadOnlyList<short> Parts
    {
        get
        {
            lock (_lock)
            {
                return _vehiclePartDamages.Select(x => x.PartId).ToList();
            }
        }
    }

    public RealmVehicle Vehicle { get; }
    public VehiclePartDamageService(VehicleContext vehicleContext, IVehiclePersistanceService vehiclePersistanceService)
    {
        Vehicle = vehicleContext.Vehicle;
        vehiclePersistanceService.Loaded += HandleLoaded;
    }

    private void HandleLoaded(IVehiclePersistanceService persistance, RealmVehicle vehicle)
    {
        _vehiclePartDamages = persistance.VehicleData.PartDamages;
    }

    public void AddPart(short partId, float state)
    {
        lock (_lock)
        {
            if (state < 0)
                throw new ArgumentOutOfRangeException(nameof(state));
            var exists = _vehiclePartDamages.FirstOrDefault(x => x.PartId == partId);
            if(exists != null)
                throw new ArgumentException("Part id already added");

            _vehiclePartDamages.Add(new VehiclePartDamageData
            {
                PartId = partId,
                State = state
            });
        }

        if (state == 0)
            PartDestroyed?.Invoke(this, partId);
    }

    public void RemovePart(short partId)
    {
        lock (_lock)
        {
            var vehiclePartDamage = _vehiclePartDamages.FirstOrDefault(x => x.PartId == partId);
            if (vehiclePartDamage == null)
                throw new ArgumentException("Part id doesn't exists");
            _vehiclePartDamages.Remove(vehiclePartDamage);
        }
        PartDestroyed?.Invoke(this, partId);
    }

    public void Modify(short partId, float difference)
    {
        float newState = 0;
        lock (_lock)
        {
            var vehiclePartDamage = _vehiclePartDamages.FirstOrDefault(x => x.PartId == partId);
            if (vehiclePartDamage == null)
                throw new ArgumentException("Part id doesn't exists");

            vehiclePartDamage.State += difference;
            if (vehiclePartDamage.State <= 0)
                vehiclePartDamage.State = 0;
            newState = vehiclePartDamage.State;
        }

        if(newState == 0)
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
}

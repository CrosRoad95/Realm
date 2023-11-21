namespace RealmCore.Server.Services.Vehicles;

internal class VehicleAccessService : IVehicleAccessService
{
    private ICollection<VehicleUserAccessData> _userAccesses = [];
    private int VehicleId { get; set; }
    private readonly object _lock = new();

    public RealmVehicle Vehicle { get; }

    public VehicleAccessService(VehicleContext vehicleContext, IVehiclePersistanceService vehiclePersistanceService)
    {
        Vehicle = vehicleContext.Vehicle;
        vehiclePersistanceService.Loaded += HandleLoaded;
    }

    private void HandleLoaded(IVehiclePersistanceService persistance, RealmVehicle vehicle)
    {
        _userAccesses = persistance.VehicleData.UserAccesses;
        VehicleId = persistance.Id;
    }

    public IReadOnlyList<VehicleUserAccessDTO> Owners
    {
        get
        {
            lock (_lock)
                return new List<VehicleUserAccessDTO>(_userAccesses.Where(x => x.AccessType == 0).Select(VehicleUserAccessDTO.Map));
        }
    }

    public bool TryGetAccess(RealmPlayer player, out VehicleUserAccessDTO vehicleAccess)
    {
        var userId = player.UserId;
        lock (_lock)
        {
            var vehicleUserAccessData = _userAccesses.Where(x => x.UserId == userId).FirstOrDefault();
            if(vehicleUserAccessData != null)
            {
                vehicleAccess = VehicleUserAccessDTO.Map(vehicleUserAccessData);
                return true;
            }
        }
        vehicleAccess = default;
        return false;
    }

    public bool HasAccess(RealmPlayer player)
    {
        var userId = player.UserId;
        lock (_lock)
        {
            var vehicleUserAccessData = _userAccesses.Where(x => x.UserId == userId).FirstOrDefault();
            if (vehicleUserAccessData != null)
            {
                return true;
            }
        }
        return false;
    }

    public VehicleUserAccessDTO AddAccess(RealmPlayer player, byte accessType, string? customValue = null)
    {
        if (TryGetAccess(player, out var _))
            throw new VehicleAccessDefinedException();

        lock (_lock)
        {
            var vehicleUserAccessData = new VehicleUserAccessData
            {
                UserId = player.UserId,
                AccessType = accessType,
                CustomValue = customValue,
                VehicleId = VehicleId
            };

            _userAccesses.Add(vehicleUserAccessData);
            return VehicleUserAccessDTO.Map(vehicleUserAccessData);
        }
    }

    public VehicleUserAccessDTO AddAsOwner(RealmPlayer player, string? customValue = null)
    {
        return AddAccess(player, 0, customValue);
    }

    public IEnumerator<VehicleUserAccessDTO> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

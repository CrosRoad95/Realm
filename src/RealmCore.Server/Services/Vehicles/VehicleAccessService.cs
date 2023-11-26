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

    private bool InternalHasAccess(int userId)
    {
        var vehicleUserAccessData = _userAccesses.Where(x => x.UserId == userId).FirstOrDefault();
        if (vehicleUserAccessData != null)
        {
            return true;
        }
        return false;
    }

    public bool TryGetAccess(int userId, out VehicleUserAccessDTO vehicleAccess)
    {
        lock (_lock)
        {
            var vehicleUserAccessData = _userAccesses.Where(x => x.UserId == userId).FirstOrDefault();
            if (vehicleUserAccessData != null)
            {
                vehicleAccess = VehicleUserAccessDTO.Map(vehicleUserAccessData);
                return true;
            }
        }
        vehicleAccess = default;
        return false;
    }

    public bool TryGetAccess(RealmPlayer player, out VehicleUserAccessDTO vehicleAccess)
    {
        var userId = player.UserId;
        return TryGetAccess(userId, out vehicleAccess);
    }

    public bool HasAccess(RealmPlayer player)
    {
        lock (_lock)
            return InternalHasAccess(player.UserId);
    }

    public bool HasAccess(int userId)
    {
        lock (_lock)
            return InternalHasAccess(userId);
    }

    public VehicleUserAccessDTO AddAccess(int userId, byte accessType, string? customValue = null)
    {
        lock (_lock)
        {
            if(InternalHasAccess(userId))
                throw new VehicleAccessDefinedException();

            var vehicleUserAccessData = new VehicleUserAccessData
            {
                UserId = userId,
                AccessType = accessType,
                CustomValue = customValue,
                VehicleId = VehicleId
            };

            _userAccesses.Add(vehicleUserAccessData);
            return VehicleUserAccessDTO.Map(vehicleUserAccessData);
        }
    }

    public VehicleUserAccessDTO AddAccess(RealmPlayer player, byte accessType, string? customValue = null)
    {
        return AddAccess(player.UserId, accessType, customValue);
    }

    public VehicleUserAccessDTO AddAsOwner(RealmPlayer player, string? customValue = null)
    {
        return AddAccess(player, 0, customValue);
    }

    public VehicleUserAccessDTO AddAsOwner(int userId, string? customValue = null)
    {
        return AddAccess(userId, 0, customValue);
    }

    public bool IsOwner(int userId)
    {
        if (TryGetAccess(userId, out var access))
            return access.AccessType == 0;
        return false;
    }

    public bool IsOwner(RealmPlayer player) => IsOwner(player.UserId);

    public IEnumerator<VehicleUserAccessDTO> GetEnumerator()
    {
        lock (_lock)
            return new List<VehicleUserAccessDTO>(_userAccesses.Select(VehicleUserAccessDTO.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

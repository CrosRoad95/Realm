using RealmCore.Server.Dto;

namespace RealmCore.Server.Services.Vehicles;

public interface IVehicleAccessService : IVehicleService, IEnumerable<VehicleUserAccessDto>
{
    IReadOnlyList<VehicleUserAccessDto> Owners { get; }

    VehicleUserAccessDto AddAccess(int userId, byte accessType, string? customValue = null);
    VehicleUserAccessDto AddAccess(RealmPlayer player, byte accessType, string? customValue = null);
    VehicleUserAccessDto AddAsOwner(RealmPlayer player, string? customValue = null);
    VehicleUserAccessDto AddAsOwner(int userId, string? customValue = null);
    bool HasAccess(RealmPlayer player);
    bool HasAccess(int userId);
    bool IsOwner(int userId);
    bool IsOwner(RealmPlayer player);
    bool TryGetAccess(RealmPlayer player, out VehicleUserAccessDto vehicleAccess);
}

internal sealed class VehicleAccessService : IVehicleAccessService
{
    private readonly object _lock = new();
    private ICollection<VehicleUserAccessData> _userAccesses = [];
    private int VehicleId { get; set; }

    public RealmVehicle Vehicle { get; init; }

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

    public IReadOnlyList<VehicleUserAccessDto> Owners
    {
        get
        {
            lock (_lock)
                return new List<VehicleUserAccessDto>(_userAccesses.Where(x => x.AccessType == 0).Select(VehicleUserAccessDto.Map));
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

    public bool TryGetAccess(int userId, out VehicleUserAccessDto vehicleAccess)
    {
        lock (_lock)
        {
            var vehicleUserAccessData = _userAccesses.Where(x => x.UserId == userId).FirstOrDefault();
            if (vehicleUserAccessData != null)
            {
                vehicleAccess = VehicleUserAccessDto.Map(vehicleUserAccessData);
                return true;
            }
        }
        vehicleAccess = default;
        return false;
    }

    public bool TryGetAccess(RealmPlayer player, out VehicleUserAccessDto vehicleAccess)
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

    public VehicleUserAccessDto AddAccess(int userId, byte accessType, string? customValue = null)
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
            return VehicleUserAccessDto.Map(vehicleUserAccessData);
        }
    }

    public VehicleUserAccessDto AddAccess(RealmPlayer player, byte accessType, string? customValue = null)
    {
        return AddAccess(player.UserId, accessType, customValue);
    }

    public VehicleUserAccessDto AddAsOwner(RealmPlayer player, string? customValue = null)
    {
        return AddAccess(player, 0, customValue);
    }

    public VehicleUserAccessDto AddAsOwner(int userId, string? customValue = null)
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

    public IEnumerator<VehicleUserAccessDto> GetEnumerator()
    {
        lock (_lock)
            return new List<VehicleUserAccessDto>(_userAccesses.Select(VehicleUserAccessDto.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

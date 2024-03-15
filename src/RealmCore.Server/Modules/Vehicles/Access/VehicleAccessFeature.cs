using RealmCore.Server.Modules.Vehicles.Persistence;

namespace RealmCore.Server.Modules.Vehicles.Access;

public interface IVehicleAccessFeature : IVehicleFeature, IEnumerable<VehicleUserAccessDto>
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

internal sealed class VehicleAccessFeature : IVehicleAccessFeature, IUsesVehiclePersistentData
{
    private readonly object _lock = new();
    private ICollection<VehicleUserAccessData> _userAccesses = [];

    public event Action? VersionIncreased;

    private int? VehicleId { get; set; }

    public RealmVehicle Vehicle { get; init; }

    public VehicleAccessFeature(VehicleContext vehicleContext)
    {
        Vehicle = vehicleContext.Vehicle;
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
        var userId = player.PersistentId;
        return TryGetAccess(userId, out vehicleAccess);
    }

    public bool HasAccess(RealmPlayer player)
    {
        lock (_lock)
            return InternalHasAccess(player.PersistentId);
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
            if (InternalHasAccess(userId))
                throw new VehicleAccessDefinedException();

            var vehicleUserAccessData = new VehicleUserAccessData
            {
                UserId = userId,
                AccessType = accessType,
                CustomValue = customValue,
                VehicleId = VehicleId ?? 0
            };

            _userAccesses.Add(vehicleUserAccessData);
            return VehicleUserAccessDto.Map(vehicleUserAccessData);
        }
    }

    public VehicleUserAccessDto AddAccess(RealmPlayer player, byte accessType, string? customValue = null)
    {
        return AddAccess(player.PersistentId, accessType, customValue);
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

    public bool IsOwner(RealmPlayer player) => IsOwner(player.PersistentId);

    public IEnumerator<VehicleUserAccessDto> GetEnumerator()
    {
        lock (_lock)
            return new List<VehicleUserAccessDto>(_userAccesses.Select(VehicleUserAccessDto.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Loaded(VehicleData vehicleData)
    {
        _userAccesses = vehicleData.UserAccesses;
        VehicleId = vehicleData.Id;
    }

    public void Unloaded()
    {
        _userAccesses = [];
        VehicleId = null;
    }
}

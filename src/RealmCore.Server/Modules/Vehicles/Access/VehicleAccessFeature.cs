namespace RealmCore.Server.Modules.Vehicles.Access;

public sealed class VehicleAccessFeature : IVehicleFeature, IEnumerable<VehicleAccessDto>, IUsesVehiclePersistentData
{
    private readonly object _lock = new();
    private ICollection<VehicleUserAccessData> _userAccesses = [];
    private ICollection<VehicleGroupAccessData> _groupAccesses = [];

    public event Action? VersionIncreased;

    private int? VehicleId { get; set; }

    public RealmVehicle Vehicle { get; init; }

    public VehicleAccessFeature(VehicleContext vehicleContext)
    {
        Vehicle = vehicleContext.Vehicle;
    }

    public VehicleUserAccessDto[] Owners
    {
        get
        {
            lock (_lock)
                return [.. _userAccesses.Where(x => x.AccessType == 0).Select(VehicleUserAccessDto.Map)];
        }
    }
    
    public VehicleUserAccessDto[] Users
    {
        get
        {
            lock (_lock)
                return [.. _userAccesses.Select(VehicleUserAccessDto.Map)];
        }
    }

    public VehicleGroupAccessDto[] Groups
    {
        get
        {
            lock (_lock)
                return [.. _groupAccesses.Select(VehicleGroupAccessDto.Map)];
        }
    }

    private bool InternalHasUserAccess(int userId, int? accessType = null)
    {
        var vehicleAccess = _userAccesses
            .Where(x => x.UserId == userId && (accessType == null || x.AccessType == accessType))
            .FirstOrDefault();

        return vehicleAccess != null;
    }
    
    private bool InternalHasGroupAccess(int groupId, int? accessType = null)
    {
        var vehicleAccess = _groupAccesses
            .Where(x => x.GroupId == groupId && (accessType == null || x.AccessType == accessType))
            .FirstOrDefault();

        return vehicleAccess != null;
    }

    public bool TryGetUserAccess(int userId, out VehicleUserAccessDto vehicleAccess)
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
        vehicleAccess = default!;
        return false;
    }
    
    public bool TryGetGroupAccess(int groupId, out VehicleGroupAccessDto vehicleAccess)
    {
        lock (_lock)
        {
            var vehicleGroupAccessData = _groupAccesses.Where(x => x.GroupId == groupId).FirstOrDefault();
            if (vehicleGroupAccessData != null)
            {
                vehicleAccess = VehicleGroupAccessDto.Map(vehicleGroupAccessData);
                return true;
            }
        }
        vehicleAccess = default!;
        return false;
    }

    public bool TryGetAccess(RealmPlayer player, out VehicleUserAccessDto vehicleAccess)
    {
        var userId = player.UserId;
        return TryGetUserAccess(userId, out vehicleAccess);
    }

    public bool HasAccess(RealmPlayer player)
    {
        lock (_lock)
            return InternalHasUserAccess(player.UserId);
    }

    public bool HasUserAccess(int userId)
    {
        lock (_lock)
            return InternalHasUserAccess(userId);
    }
    
    public bool HasGroupAccess(int groupId)
    {
        lock (_lock)
            return InternalHasGroupAccess(groupId);
    }

    public VehicleUserAccessDto? TryAddUserAccess(int userId, int accessType, object? metadata = null)
    {
        var vehicleAccess = new VehicleUserAccessData
        {
            UserId = userId,
            AccessType = accessType,
            Metadata = JsonHelpers.Serialize(metadata),
            VehicleId = VehicleId ?? 0
        };

        lock (_lock)
        {
            if (InternalHasUserAccess(userId))
                return null;

            _userAccesses.Add(vehicleAccess);
        }

        VersionIncreased?.Invoke();

        return VehicleUserAccessDto.Map(vehicleAccess);
    }
    
    public VehicleGroupAccessDto? TryAddGroupAccess(int groupId, int accessType, object? metadata = null)
    {
        var vehicleAccess = new VehicleGroupAccessData
        {
            GroupId = groupId,
            AccessType = accessType,
            Metadata = JsonHelpers.Serialize(metadata),
            VehicleId = VehicleId ?? 0
        };

        lock (_lock)
        {
            if (InternalHasGroupAccess(groupId))
                return null;

            _groupAccesses.Add(vehicleAccess);
        }

        VersionIncreased?.Invoke();

        return VehicleGroupAccessDto.Map(vehicleAccess);
    }

    public VehicleUserAccessDto? TryAddAccess(RealmPlayer player, int accessType, object? metadata = null)
    {
        return TryAddUserAccess(player.UserId, accessType, metadata);
    }

    public VehicleUserAccessDto? TryAddAsOwner(RealmPlayer player, object? metadata = null)
    {
        return TryAddAccess(player, 0, metadata);
    }

    public VehicleUserAccessDto? TryAddUserAsOwner(int userId, object? metadata = null)
    {
        return TryAddUserAccess(userId, 0, metadata);
    }

    public bool IsUserOwner(int userId)
    {
        if (TryGetUserAccess(userId, out var access))
            return access.AccessType == 0;
        return false;
    }

    public bool IsOwner(RealmPlayer player) => IsUserOwner(player.UserId);

    public bool TryRemoveUserAccess(int userId, int? accessType = null)
    {
        lock (_lock)
        {
            var vehicleAccess = _userAccesses
                .Where(x => x.UserId == userId && (accessType == null || x.AccessType == accessType))
                .FirstOrDefault();

            if (vehicleAccess == null)
                return false;

            _userAccesses.Remove(vehicleAccess);
            return true;
        }
    }
    
    public bool TryRemoveGroupAccess(int groupId, int? accessType = null)
    {
        lock (_lock)
        {
            var vehicleAccess = _groupAccesses
                .Where(x => x.GroupId == groupId && (accessType == null || x.AccessType == accessType))
                .FirstOrDefault();

            if (vehicleAccess == null)
                return false;

            _groupAccesses.Remove(vehicleAccess);
            return true;
        }
    }

    public bool TryRemoveAccess(RealmPlayer player, int? accessType = null) => TryRemoveUserAccess(player.UserId, accessType);

    public IEnumerator<VehicleAccessDto> GetEnumerator()
    {
        VehicleUserAccessData[] userAccesses;
        VehicleGroupAccessData[] groupAccesses;

        lock (_lock)
        {
            userAccesses = [.. _userAccesses];
            groupAccesses = [.. _groupAccesses];
        }

        foreach (var userAccessData in userAccesses)
            yield return VehicleUserAccessDto.Map(userAccessData);
        
        foreach (var groupAccessData in groupAccesses)
            yield return VehicleGroupAccessDto.Map(groupAccessData);

    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Loaded(VehicleData vehicleData, bool preserveData = false)
    {
        _userAccesses = vehicleData.UserAccesses;
        _groupAccesses = vehicleData.GroupAccesses;
        VehicleId = vehicleData.Id;
    }

    public void Unloaded()
    {
        _userAccesses = [];
        _groupAccesses = [];
        VehicleId = null;
    }
}

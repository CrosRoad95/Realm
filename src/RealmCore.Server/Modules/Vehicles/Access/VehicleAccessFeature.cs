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

    private VehicleUserAccessData? InternalGetUserAccess(int userId, int? accessType = null)
    {
        var vehicleAccess = _userAccesses
            .Where(x => x.UserId == userId && (accessType == null || x.AccessType == accessType))
            .FirstOrDefault();

        return vehicleAccess;
    }
    
    private VehicleGroupAccessData? InternalGetGroupAccess(int groupId, int? accessType = null)
    {
        var vehicleAccess = _groupAccesses
            .Where(x => x.GroupId == groupId && (accessType == null || x.AccessType == accessType))
            .FirstOrDefault();

        return vehicleAccess;
    }
    
    private bool InternalHasUserAccess(int userId, int? accessType = null)
    {
        return InternalGetUserAccess(userId, accessType) != null;
    }
    
    private bool InternalHasGroupAccess(int groupId, int? accessType = null)
    {
        return InternalGetGroupAccess(groupId, accessType) != null;
    }

    public VehicleUserAccessDto[] GetUserAccess(int userId)
    {
        lock (_lock)
        {
            var vehicleUserAccessData = _userAccesses.Where(x => x.UserId == userId);
            return vehicleUserAccessData.Select(VehicleUserAccessDto.Map).ToArray();
        }
    }
    
    public VehicleGroupAccessDto[] GetGroupAccess(int groupId)
    {
        lock (_lock)
        {
            var vehicleGroupAccessData = _groupAccesses.Where(x => x.GroupId == groupId);
            return vehicleGroupAccessData.Select(VehicleGroupAccessDto.Map).ToArray();
        }
    }

    public VehicleUserAccessDto[] GetUserAccess(RealmPlayer player)
    {
        return GetUserAccess(player.UserId);
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
            if (InternalHasUserAccess(userId, accessType))
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
            if (InternalHasGroupAccess(groupId, accessType))
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

    public bool SetUserAccessMetadata(int userId, int accessType, object? metadata = null)
    {
        lock (_lock)
        {
            var userAccess = InternalGetUserAccess(userId, accessType);
            if (userAccess == null)
                return false;

            userAccess.Metadata = JsonHelpers.Serialize(metadata);
        }

        VersionIncreased?.Invoke();
        return true;
    }
    
    public bool SetGroupAccessMetadata(int groupId, int accessType, object? metadata = null)
    {
        lock (_lock)
        {
            var groupAccess = InternalGetGroupAccess(groupId, accessType);
            if (groupAccess == null)
                return false;

            groupAccess.Metadata = JsonHelpers.Serialize(metadata);
        }

        VersionIncreased?.Invoke();
        return true;
    }

    public VehicleUserAccessDto? TryAddUserAsOwner(int userId, object? metadata = null)
    {
        return TryAddUserAccess(userId, 0, metadata);
    }

    public bool IsUserOwner(int userId)
    {
        return GetUserAccess(userId).Any(x => x.AccessType == 0);
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

    public void Clear()
    {
        _userAccesses = [];
        _groupAccesses = [];
    }

    public void Unloaded()
    {
        Clear();
        VehicleId = null;
    }
}

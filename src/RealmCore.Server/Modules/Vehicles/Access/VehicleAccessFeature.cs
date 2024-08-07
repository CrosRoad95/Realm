﻿namespace RealmCore.Server.Modules.Vehicles.Access;

public sealed class VehicleAccessFeature : IVehicleFeature, IEnumerable<VehicleUserAccessDto>, IUsesVehiclePersistentData
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

    public VehicleUserAccessDto[] Owners
    {
        get
        {
            lock (_lock)
                return [.. _userAccesses.Where(x => x.AccessType == 0).Select(VehicleUserAccessDto.Map)];
        }
    }

    private bool InternalHasAccess(int userId, byte? accessType = null)
    {
        var vehicleUserAccessData = _userAccesses
            .Where(x => x.UserId == userId && (accessType == null || x.AccessType == accessType))
            .FirstOrDefault();
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

    public bool TryRemoveAccess(int userId, byte? accessType = null)
    {
        lock (_lock)
        {
            var vehicleUserAccessData = _userAccesses
                .Where(x => x.UserId == userId && (accessType == null || x.AccessType == accessType))
                .FirstOrDefault();

            if (vehicleUserAccessData == null)
                return false;

            _userAccesses.Remove(vehicleUserAccessData);
            return true;
        }
    }

    public bool TryRemoveAccess(RealmPlayer player, byte? accessType = null) => TryRemoveAccess(player.UserId, accessType);

    public IEnumerator<VehicleUserAccessDto> GetEnumerator()
    {
        VehicleUserAccessData[] view;

        lock (_lock)
            view = [.. _userAccesses];

        foreach (var userAccessData in view)
        {
            yield return VehicleUserAccessDto.Map(userAccessData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Loaded(VehicleData vehicleData, bool preserveData = false)
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

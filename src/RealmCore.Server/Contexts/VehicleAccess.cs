namespace RealmCore.Server.Contexts;

public class VehicleAccess
{
    private readonly List<VehiclePlayerAccess> _vehiclePlayerAccesses;
    private readonly object _lock = new();

    public VehicleAccess(IEnumerable<VehicleUserAccessData> vehicleUserAccessData)
    {
        _vehiclePlayerAccesses = vehicleUserAccessData.Select(x => new VehiclePlayerAccess
        {
            id = x.Id,
            userId = x.User.Id,
            accessType = x.AccessType,
            customValue = x.CustomValue
        }).ToList();
    }

    public IReadOnlyList<VehiclePlayerAccess> PlayerAccesses
    {
        get
        {
            lock (_lock)
                return new List<VehiclePlayerAccess>(_vehiclePlayerAccesses);
        }
    }

    public IReadOnlyList<VehiclePlayerAccess> Owners
    {
        get
        {
            lock (_lock)
                return new List<VehiclePlayerAccess>(_vehiclePlayerAccesses.Where(x => x.accessType == 0));
        }
    }

    public bool TryGetAccess(RealmPlayer player, out VehiclePlayerAccess vehicleAccess)
    {
        var userId = player.GetUserId();
        lock (_lock)
        {
            var index = _vehiclePlayerAccesses.FindIndex(x => x.userId == userId);
            if (index >= 0)
            {
                vehicleAccess = _vehiclePlayerAccesses[index];
                return true;
            }
        }
        vehicleAccess = default;
        return false;
    }

    public bool HasAccess(RealmPlayer player)
    {
        if (TryGetAccess(player, out VehiclePlayerAccess vehiclePlayerAccess))
        {
            return true;
        }
        return false;
    }

    public VehiclePlayerAccess AddAccess(RealmPlayer player, byte accessType, string? customValue = null)
    {
        if (TryGetAccess(player, out var _))
            throw new VehicleAccessDefinedException();

        lock (_lock)
        {
            _vehiclePlayerAccesses.Add(new VehiclePlayerAccess
            {
                userId = player.GetUserId(),
                accessType = accessType,
                customValue = customValue
            });
            return _vehiclePlayerAccesses.Last();
        }
    }

    public VehiclePlayerAccess AddAsOwner(RealmPlayer player, string? customValue = null)
    {
        return AddAccess(player, 0, customValue);
    }
}

namespace RealmCore.Server.Modules.Vehicles.Tuning;

public sealed class VehicleUpgradesFeature : IVehicleFeature, IEnumerable<int>, IUsesVehiclePersistentData
{
    private readonly object _lock = new();
    private ICollection<VehicleUpgradeData> _upgrades = [];


    public event Action<VehicleUpgradesFeature, int>? UpgradeAdded;
    public event Action<VehicleUpgradesFeature, int>? UpgradeRemoved;
    public event Action<VehicleUpgradesFeature>? Rebuild;
    public event Action? VersionIncreased;

    public RealmVehicle Vehicle { get; init; }

    public VehicleUpgradesFeature(VehicleContext vehicleContext)
    {
        Vehicle = vehicleContext.Vehicle;
    }

    public void ForceRebuild() => Rebuild?.Invoke(this);

    private bool InternalHasUpgrade(int upgradeId) => _upgrades.Any(x => x.UpgradeId == upgradeId);

    public bool HasUpgrade(int upgradeId)
    {
        lock (_lock)
            return InternalHasUpgrade(upgradeId);
    }

    public bool AddUpgrades(IEnumerable<int> upgradeIds, bool rebuild = true)
    {
        lock (_lock)
        {
            foreach (var item in upgradeIds)
            {
                _upgrades.Add(new VehicleUpgradeData
                {
                    UpgradeId = item
                });
            }
        }
        foreach (var upgradeId in upgradeIds)
            UpgradeAdded?.Invoke(this, upgradeId);

        if (rebuild)
            Rebuild?.Invoke(this);
        return true;
    }

    public bool AddUpgrade(int upgradeId, bool rebuild = true)
    {
        lock (_lock)
        {
            _upgrades.Add(new VehicleUpgradeData
            {
                UpgradeId = upgradeId
            });
        }
        UpgradeAdded?.Invoke(this, upgradeId);
        if (rebuild)
            Rebuild?.Invoke(this);
        return true;
    }

    public bool AddUniqueUpgrade(int upgradeId, bool rebuild = true)
    {
        lock (_lock)
        {
            if (InternalHasUpgrade(upgradeId))
                return false;

            _upgrades.Add(new VehicleUpgradeData
            {
                UpgradeId = upgradeId
            });
        }
        UpgradeAdded?.Invoke(this, upgradeId);

        if (rebuild)
            Rebuild?.Invoke(this);

        return true;
    }

    public void RemoveAllUpgrades(bool rebuild = true)
    {
        List<int> copy;
        lock (_lock)
        {
            copy = new List<int>(_upgrades.Select(x => x.UpgradeId));
            _upgrades.Clear();
        }

        foreach (var upgradeId in copy)
            UpgradeRemoved?.Invoke(this, upgradeId);

        if (rebuild)
            Rebuild?.Invoke(this);
    }

    public bool RemoveUpgrade(int upgradeId, bool rebuild = true)
    {
        lock (_upgrades)
        {
            var upgrade = _upgrades.FirstOrDefault(x => x.UpgradeId == upgradeId);
            if (upgrade == null)
                return false;
            _upgrades.Remove(upgrade);
        }
        UpgradeRemoved?.Invoke(this, upgradeId);
        if (rebuild)
            Rebuild?.Invoke(this);
        return true;
    }

    public IEnumerator<int> GetEnumerator()
    {
        int[] view;
        lock (_lock)
            view = [.. _upgrades.Select(x => x.UpgradeId)];

        foreach (var upgradeId in view)
        {
            yield return upgradeId;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Loaded(VehicleData vehicleData, bool preserveData = false)
    {
        _upgrades = vehicleData.Upgrades;
    }

    public void Unloaded()
    {
    }
}

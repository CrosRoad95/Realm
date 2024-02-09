namespace RealmCore.Server.Modules.Vehicles.Vehicles;

public interface IVehicleUpgradesFeature : IVehicleFeature, IEnumerable<int>
{
    IReadOnlyCollection<int> Upgrades { get; }

    event Action<IVehicleUpgradesFeature, int>? UpgradeAdded;
    event Action<IVehicleUpgradesFeature, int>? UpgradeRemoved;
    internal event Action<IVehicleUpgradesFeature>? Rebuild;

    bool AddUniqueUpgrade(int upgradeId, bool rebuild = true);
    bool AddUpgrade(int upgradeId, bool rebuild = true);
    bool AddUpgrades(IEnumerable<int> upgradeIds, bool rebuild = true);
    void ForceRebuild();
    bool HasUpgrade(int upgradeId);
    void RemoveAllUpgrades(bool rebuild = true);
    bool RemoveUpgrade(int upgradeId, bool rebuild = true);
}

internal sealed class VehicleUpgradesFeature : IVehicleUpgradesFeature
{
    private readonly object _lock = new();
    private ICollection<VehicleUpgradeData> _upgrades = [];

    public IReadOnlyCollection<int> Upgrades
    {
        get
        {
            lock (_lock)
                return _upgrades.Select(x => x.UpgradeId).ToList().AsReadOnly();
        }
    }

    public event Action<IVehicleUpgradesFeature, int>? UpgradeAdded;
    public event Action<IVehicleUpgradesFeature, int>? UpgradeRemoved;
    public event Action<IVehicleUpgradesFeature>? Rebuild;

    public RealmVehicle Vehicle { get; init; }

    public VehicleUpgradesFeature(VehicleContext vehicleContext, IVehiclePersistanceFeature persistance)
    {
        Vehicle = vehicleContext.Vehicle;
        persistance.Loaded += HandleLoaded;
    }

    private void HandleLoaded(IVehiclePersistanceFeature persistance, RealmVehicle vehicle)
    {
        _upgrades = persistance.VehicleData.Upgrades;
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

    public IEnumerator<int> GetEnumerator() => Upgrades.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

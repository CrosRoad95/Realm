namespace RealmCore.Server.Components.Vehicles;

[ComponentUsage(false)]
public class VehicleUpgradesComponent : Component
{
    private readonly List<int> _upgrades = new();
    private readonly object _lock = new();

    public IReadOnlyCollection<int> Upgrades
    {
        get
        {
            ThrowIfDisposed();
            lock (_lock)
                return new List<int>(_upgrades).AsReadOnly();
        }
    }

    public event Action<VehicleUpgradesComponent, int>? UpgradeAdded;
    public event Action<VehicleUpgradesComponent, int>? UpgradeRemoved;
    public event Action<VehicleUpgradesComponent>? Rebuild;

    public VehicleUpgradesComponent() { }

    public VehicleUpgradesComponent(IEnumerable<int> upgrades)
    {
        _upgrades = new(upgrades);
    }

    internal VehicleUpgradesComponent(ICollection<VehicleUpgradeData> vehicleUpgrades)
    {
        _upgrades = vehicleUpgrades.Select(x => x.UpgradeId).ToList();
    }

    protected override void Attach()
    {
        if (_upgrades.Count > 0)
            Rebuild?.Invoke(this);
    }

    private bool InternalHasUpgrade(int upgradeId) => _upgrades.Any(x => x == upgradeId);

    public bool HasUpgrade(int upgradeId)
    {
        ThrowIfDisposed();
        lock (_lock)
            return InternalHasUpgrade(upgradeId);
    }

    public bool AddUpgrades(IEnumerable<int> upgradeIds, bool rebuild = true)
    {
        ThrowIfDisposed();
        lock (_lock)
        {
            _upgrades.AddRange(upgradeIds);
        }
        foreach (var upgradeId in upgradeIds)
            UpgradeAdded?.Invoke(this, upgradeId);

        if (rebuild)
            Rebuild?.Invoke(this);
        return true;
    }

    public bool AddUpgrade(int upgradeId, bool rebuild = true)
    {
        ThrowIfDisposed();
        lock (_lock)
        {
            _upgrades.Add(upgradeId);
        }
        UpgradeAdded?.Invoke(this, upgradeId);
        if (rebuild)
            Rebuild?.Invoke(this);
        return true;
    }

    public bool AddUniqueUpgrade(int upgradeId, bool rebuild = true)
    {
        ThrowIfDisposed();
        lock (_lock)
        {
            if (InternalHasUpgrade(upgradeId))
                return false;

            _upgrades.Add(upgradeId);
        }
        UpgradeAdded?.Invoke(this, upgradeId);

        if (rebuild)
            Rebuild?.Invoke(this);

        return true;
    }

    public void RemoveAllUpgrades(bool rebuild = true)
    {
        ThrowIfDisposed();
        List<int> copy;
        lock (_lock)
        {
            copy = new List<int>(_upgrades);
            _upgrades.Clear();
        }

        foreach (var upgradeId in copy)
            UpgradeRemoved?.Invoke(this, upgradeId);

        if (rebuild)
            Rebuild?.Invoke(this);
    }

    public bool RemoveUpgrade(int upgradeId, bool rebuild = true)
    {
        ThrowIfDisposed();
        bool result;
        lock (_upgrades)
        {
            result = _upgrades.Remove(upgradeId);
        }
        if (result)
        {
            UpgradeRemoved?.Invoke(this, upgradeId);
            if (rebuild)
                Rebuild?.Invoke(this);
        }
        return result;
    }
}

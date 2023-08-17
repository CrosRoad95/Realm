using RealmCore.Persistence.Data;

namespace RealmCore.Server.Components.Vehicles;

[ComponentUsage(false)]
public class VehicleUpgradesComponent : Component
{
    [Inject]
    private VehicleUpgradeRegistry VehicleUpgradeRegistry { get; set; } = default!;
    [Inject]
    private VehicleEnginesRegistry VehicleEnginesRegistry { get; set; } = default!;

    private readonly List<int> _upgrades = new();
    private readonly object _lock = new();
    private bool _dirtyState = false;

    public IReadOnlyCollection<int> Upgrades
    {
        get
        {
            ThrowIfDisposed();
            lock (_lock)
                return new List<int>(_upgrades).AsReadOnly();
        }
    }

    public byte Paintjob
    {
        get
        {
            ThrowIfDisposed();
            return Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.PaintJob;
        }
        set
        {
            ThrowIfDisposed();
            Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.PaintJob = value;
            PaintjobChanged?.Invoke(this, value);
        }
    }

    public event Action<VehicleUpgradesComponent, int>? UpgradeAdded;
    public event Action<VehicleUpgradesComponent, int>? UpgradeRemoved;
    public event Action<VehicleUpgradesComponent, byte>? PaintjobChanged;

    public VehicleUpgradesComponent() { }

    internal VehicleUpgradesComponent(ICollection<VehicleUpgradeData> vehicleUpgrades)
    {
        _upgrades = vehicleUpgrades.Select(x => x.UpgradeId).ToList();
        _dirtyState = true;
    }

    protected override void Load()
    {
        RebuildUpgrades();
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
            _dirtyState = true;
        }
        foreach (var upgradeId in upgradeIds)
            UpgradeAdded?.Invoke(this, upgradeId);

        if (rebuild)
            RebuildUpgrades();
        return true;
    }

    public bool AddUpgrade(int upgradeId, bool rebuild = true)
    {
        ThrowIfDisposed();
        lock (_lock)
        {
            _upgrades.Add(upgradeId);
            _dirtyState = true;
        }
        UpgradeAdded?.Invoke(this, upgradeId);
        if (rebuild)
            RebuildUpgrades();
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
            _dirtyState = true;
        }
        UpgradeAdded?.Invoke(this, upgradeId);
        if (rebuild)
            RebuildUpgrades();
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
            _dirtyState = true;
        }

        foreach (var upgradeId in copy)
            UpgradeRemoved?.Invoke(this, upgradeId);

        if (rebuild)
            RebuildUpgrades();
    }

    public bool RemoveUpgrade(int upgradeId, bool rebuild = true)
    {
        ThrowIfDisposed();
        bool result;
        lock (_upgrades)
        {
            result = _upgrades.Remove(upgradeId);
            _dirtyState = true;
        }
        if (result)
        {
            UpgradeRemoved?.Invoke(this, upgradeId);
            if (rebuild)
                RebuildUpgrades();
        }
        return result;
    }

    private void ApplyVisualUpgrades(VehicleUpgrades vehicleUpgrades, IEnumerable<VehicleUpgradeRegistryEntry?> upgradeDescriptions)
    {
        if (!upgradeDescriptions.Any())
            throw new InvalidOperationException("Sequence contains no elements");

        var visualUpgrades = upgradeDescriptions.Where(x => x?.Visuals != null).Select(x => x!.Visuals!).ToList();
        if (visualUpgrades.Any())
        {
            vehicleUpgrades.Hood = visualUpgrades.Where(x => x.Hood != null).Select(x => x.Hood).FirstOrDefault() ?? 0;
            vehicleUpgrades.Vent = visualUpgrades.Where(x => x.Vent != null).Select(x => x.Vent).FirstOrDefault() ?? 0;
            vehicleUpgrades.Spoiler = visualUpgrades.Where(x => x.Spoiler != null).Select(x => x.Spoiler).FirstOrDefault() ?? 0;
            vehicleUpgrades.Sideskirt = visualUpgrades.Where(x => x.Sideskirt != null).Select(x => x.Sideskirt).FirstOrDefault() ?? 0;
            vehicleUpgrades.FrontBullbar = visualUpgrades.Where(x => x.FrontBullbar != null).Select(x => x.FrontBullbar).FirstOrDefault() ?? 0;
            vehicleUpgrades.RearBullbar = visualUpgrades.Where(x => x.RearBullbar != null).Select(x => x.RearBullbar).FirstOrDefault() ?? 0;
            vehicleUpgrades.Lamps = visualUpgrades.Where(x => x.Lamps != null).Select(x => x.Lamps).FirstOrDefault() ?? 0;
            vehicleUpgrades.Roof = visualUpgrades.Where(x => x.Roof != null).Select(x => x.Roof).FirstOrDefault() ?? 0;
            vehicleUpgrades.Nitro = visualUpgrades.Where(x => x.Nitro != null).Select(x => x.Nitro).FirstOrDefault() ?? 0;
            vehicleUpgrades.HasHydraulics = visualUpgrades.Where(x => x.HasHydraulics != null).Select(x => x.HasHydraulics).FirstOrDefault() ?? false;
            vehicleUpgrades.HasStereo = visualUpgrades.Where(x => x.HasStereo != null).Select(x => x.HasStereo).FirstOrDefault() ?? false;
            vehicleUpgrades.Wheels = visualUpgrades.Where(x => x.Wheels != null).Select(x => x.Wheels).FirstOrDefault() ?? 0;
            vehicleUpgrades.Exhaust = visualUpgrades.Where(x => x.Exhaust != null).Select(x => x.Exhaust).FirstOrDefault() ?? 0;
            vehicleUpgrades.FrontBumper = visualUpgrades.Where(x => x.FrontBumper != null).Select(x => x.FrontBumper).FirstOrDefault() ?? 0;
            vehicleUpgrades.RearBumper = visualUpgrades.Where(x => x.RearBumper != null).Select(x => x.RearBumper).FirstOrDefault() ?? 0;
            vehicleUpgrades.Misc = visualUpgrades.Where(x => x.Misc != null).Select(x => x.Misc).FirstOrDefault() ?? 0;
        }
    }

    private void ApplyUpgrade(object vehicleHandling, IEnumerable<FloatValueUpgradeDescription?> upgradeDescriptions, Expression<Func<VehicleHandling, float>> handlingProperty)
    {
        if (!upgradeDescriptions.Any())
            throw new InvalidOperationException("Sequence contains no elements");

        float increaseByUnits = 0;
        float multipleBy = 0;
        var memberExpression = (MemberExpression)handlingProperty.Body;
        var propertyInfo = (PropertyInfo)memberExpression.Member;
        var value = (float)propertyInfo.GetValue(vehicleHandling)!;
        foreach (var floatValueUpgradeDescription in upgradeDescriptions.Where(x => x != null).Select(x => x!))
        {
            increaseByUnits += floatValueUpgradeDescription.IncreaseByUnits;
            multipleBy += floatValueUpgradeDescription.MultipleBy;
            if (floatValueUpgradeDescription.MultipleBy == 0)
                throw new ArgumentOutOfRangeException(nameof(floatValueUpgradeDescription.MultipleBy), "Multiple by can not be 0");
        }
        if (multipleBy != 0)
            value *= multipleBy;
        value += increaseByUnits;

        propertyInfo.SetValue(vehicleHandling, value);
    }

    private void ApplyUpgrades(object boxedVehicleHandling, IEnumerable<VehicleUpgradeRegistryEntry> upgradesEntries)
    {
        ApplyUpgrade(boxedVehicleHandling, upgradesEntries.Select(x => x.MaxVelocity), x => x.MaxVelocity);
        ApplyUpgrade(boxedVehicleHandling, upgradesEntries.Select(x => x.EngineAcceleration), x => x.EngineAcceleration);
    }

    public void RebuildUpgrades()
    {
        ThrowIfDisposed();

        var vehicle = Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle;
        var vehicleHandling = VehicleHandlingConstants.DefaultVehicleHandling[vehicle.Model];
        object boxedVehicleHandling = vehicleHandling;

        lock (_lock)
        {
            if (!_dirtyState)
                return;

            if (_upgrades.Any())
            {
                IEnumerable<VehicleUpgradeRegistryEntry> upgradesEntries = _upgrades.Select(VehicleUpgradeRegistry.Get);

                ApplyUpgrades(boxedVehicleHandling, upgradesEntries);
                ApplyVisualUpgrades(vehicle.Upgrades, upgradesEntries);

                if (Entity.TryGetComponent(out VehicleEngineComponent vehicleEngineComponent))
                {
                    var upgradeId = VehicleEnginesRegistry.Get(vehicleEngineComponent.ActiveVehicleEngineId).UpgradeId;
                    ApplyUpgrades(boxedVehicleHandling, new VehicleUpgradeRegistryEntry[] { VehicleUpgradeRegistry.Get(upgradeId) });
                }
            }
            _dirtyState = false;
        }

        vehicle.Handling = (VehicleHandling)boxedVehicleHandling;
    }
}

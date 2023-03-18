using Newtonsoft.Json.Linq;
using Realm.Domain.Data;
using SlipeServer.Packets.Definitions.Entities.Structs;
using VehicleUpgradeData = Realm.Persistance.Data.VehicleUpgrade;

namespace Realm.Domain.Components.Vehicles;

[ComponentUsage(false)]
public class VehicleUpgradesComponent : Component
{
    [Inject]
    private VehicleUpgradeRegistry VehicleUpgradeRegistry { get; set; } = default!;

    private readonly List<int> _upgrades = new();
    private readonly object _upgradesLock = new();
    private bool _dirtyState = false;

    public IReadOnlyCollection<int> Upgrades
    {
        get
        {
            lock(_upgradesLock)
            {
                return new List<int>(_upgrades);
            }
        }
    }

    public event Action<VehicleUpgradesComponent, int>? UpgradeAdded;
    public event Action<VehicleUpgradesComponent, int>? UpgradeRemoved;

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
        lock (_upgradesLock)
            return InternalHasUpgrade(upgradeId);
    }

    public bool AddUpgrades(IEnumerable<int> upgradeIds, bool rebuild = true)
    {
        lock (_upgradesLock)
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
        lock (_upgradesLock)
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
        lock (_upgradesLock)
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
        List<int> copy;
        lock (_upgradesLock)
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
        bool result;
        lock(_upgrades)
        {
            result = _upgrades.Remove(upgradeId);
            _dirtyState = true;
        }
        if (result)
        {
            UpgradeRemoved?.Invoke(this, upgradeId);
            if(rebuild)
                RebuildUpgrades();
        }
        return result;
    }

    private void ApplyUpgrade(object vehicleHandling, IEnumerable<FloatValueUpgradeDescription?> upgradeDescriptions, Expression<Func<VehicleHandling, float>> handlingProperty)
    {
        float increseByUnits = 0;
        float multipleBy = 0;
        var memberExpression = (MemberExpression)handlingProperty.Body;
        var propertyInfo = (PropertyInfo)memberExpression.Member;
        var value = (float)propertyInfo.GetValue(vehicleHandling);
        foreach (var floatValueUpgradeDescription in upgradeDescriptions.Where(x => x != null).Select(x => x!))
        {
            increseByUnits += floatValueUpgradeDescription.IncreaseByUnits;
            multipleBy += floatValueUpgradeDescription.MultipleBy;
        }
        value *= multipleBy;
        value += increseByUnits;

        propertyInfo.SetValue(vehicleHandling, value);
    }

    private void ApplyUpgrades(object boxedVehicleHandling, IEnumerable<VehicleUpgradeRegistryEntry> upgradesEntries)
    {
        ApplyUpgrade(boxedVehicleHandling, upgradesEntries.Select(x => x.MaxVelocity), x => x.MaxVelocity);
        ApplyUpgrade(boxedVehicleHandling, upgradesEntries.Select(x => x.EngineAcceleration), x => x.EngineAcceleration);
    }

    public void RebuildUpgrades()
    {
        var vehicle = Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle;
        var vehicleHandling = VehicleHandlingConstants.DefaultVehicleHandling[vehicle.Model];
        object boxedVehicleHandling = vehicleHandling;

        lock (_upgradesLock)
        {
            if (!_dirtyState)
                return;

            if (_upgrades.Any())
            {
                IEnumerable<VehicleUpgradeRegistryEntry> upgradesEntries = _upgrades.Select(VehicleUpgradeRegistry.Get);

                ApplyUpgrades(boxedVehicleHandling, upgradesEntries);

                if (Entity.TryGetComponent(out VehicleEngineComponent vehicleEngineComponent))
                {
                    ApplyUpgrades(boxedVehicleHandling, new VehicleUpgradeRegistryEntry[] { VehicleUpgradeRegistry.Get(vehicleEngineComponent.UpgradeId) });
                }
            }
            _dirtyState = false;
        }
        vehicle.Handling = (VehicleHandling)boxedVehicleHandling;
    }
}

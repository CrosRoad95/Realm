
namespace RealmCore.Server.Logic.Components;

internal sealed class VehicleUpgradesComponentLogic : ComponentLogic<VehicleUpgradesComponent>
{
    private readonly VehicleUpgradeRegistry _vehicleUpgradeRegistry;
    private readonly VehicleEnginesRegistry _vehicleEnginesRegistry;

    public VehicleUpgradesComponentLogic(IElementFactory elementFactory, VehicleUpgradeRegistry vehicleUpgradeRegistry, VehicleEnginesRegistry vehicleEnginesRegistry) : base(elementFactory)
    {
        _vehicleUpgradeRegistry = vehicleUpgradeRegistry;
        _vehicleEnginesRegistry = vehicleEnginesRegistry;
    }

    protected override void ComponentAdded(VehicleUpgradesComponent vehicleUpgradesComponent)
    {
        vehicleUpgradesComponent.Rebuild += HandleRebuild;
    }

    protected override void ComponentDetached(VehicleUpgradesComponent vehicleUpgradesComponent)
    {
        vehicleUpgradesComponent.Rebuild -= HandleRebuild;
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

    private void HandleRebuild(VehicleUpgradesComponent vehicleUpgradesComponent)
    {
        var vehicle = (RealmVehicle)vehicleUpgradesComponent.Element;
        var upgrades = vehicleUpgradesComponent.Upgrades;
        var vehicleHandling = VehicleHandlingConstants.DefaultVehicleHandling[vehicle.Model];
        object boxedVehicleHandling = vehicleHandling;

        if (upgrades.Count != 0)
        {
            IEnumerable<VehicleUpgradeRegistryEntry> upgradesEntries = upgrades.Select(_vehicleUpgradeRegistry.Get);

            ApplyUpgrades(boxedVehicleHandling, upgradesEntries);
            ApplyVisualUpgrades(vehicle.Upgrades, upgradesEntries);

            if (vehicle.Components.TryGetComponent(out VehicleEngineComponent vehicleEngineComponent))
            {
                var upgradeId = _vehicleEnginesRegistry.Get(vehicleEngineComponent.ActiveVehicleEngineId).UpgradeId;
                ApplyUpgrades(boxedVehicleHandling, new VehicleUpgradeRegistryEntry[] { _vehicleUpgradeRegistry.Get(upgradeId) });
            }
        }

        vehicle.Handling = (VehicleHandling) boxedVehicleHandling;
    }

    private void ApplyVisualUpgrades(VehicleUpgrades vehicleUpgrades, IEnumerable<VehicleUpgradeRegistryEntry?> upgradeDescriptions)
    {
        if (!upgradeDescriptions.Any())
            throw new InvalidOperationException("Sequence contains no elements");

        var visualUpgrades = upgradeDescriptions.Where(x => x?.Visuals != null).Select(x => x!.Visuals!).ToList();
        if (visualUpgrades.Count != 0)
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
}

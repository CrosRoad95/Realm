using RealmCore.Server.Concepts.Interfaces;

namespace RealmCore.Server.Logic.Components;

internal sealed class VehicleUpgradesComponentLogic : ComponentLogic<VehicleUpgradesComponent>
{
    private readonly VehicleUpgradeRegistry _vehicleUpgradeRegistry;

    public VehicleUpgradesComponentLogic(IElementFactory elementFactory, VehicleUpgradeRegistry vehicleUpgradeRegistry) : base(elementFactory)
    {
        _vehicleUpgradeRegistry = vehicleUpgradeRegistry;
    }

    protected override void ComponentAdded(VehicleUpgradesComponent vehicleUpgradesComponent)
    {
        vehicleUpgradesComponent.Rebuild += HandleRebuild;
    }

    protected override void ComponentDetached(VehicleUpgradesComponent vehicleUpgradesComponent)
    {
        vehicleUpgradesComponent.Rebuild -= HandleRebuild;
    }

    public void Execute(List<IVehicleHandlingModifier> stages, VehicleHandlingContext vehicleHandlingContext)
    {
        void Next(VehicleHandlingContext data, int index)
        {
            if (index < stages.Count)
            {
                stages[index].Apply(data, newData => Next(newData, index + 1));
            }
        }

        Next(vehicleHandlingContext, 0);
    }

    private void HandleRebuild(VehicleUpgradesComponent vehicleUpgradesComponent)
    {
        var vehicle = (RealmVehicle)vehicleUpgradesComponent.Element;
        var upgrades = vehicleUpgradesComponent.Upgrades;

        var vehicleHandlingContext = new VehicleHandlingContext(vehicle.Model);

        if (upgrades.Count != 0)
        {
            var vehicleHandlingModifiers = upgrades.Select(x => _vehicleUpgradeRegistry.Get(x))
                .Select(x => x.VehicleUpgrade)
                .ToList();

            void Next(VehicleHandlingContext data, int index)
            {
                if (index < vehicleHandlingModifiers.Count)
                {
                    vehicleHandlingModifiers[index].Apply(data, newData => Next(newData, index + 1));
                }
            }

            Next(vehicleHandlingContext, 0);
        }

        vehicle.Handling = vehicleHandlingContext.VehicleHandling;
    }

}

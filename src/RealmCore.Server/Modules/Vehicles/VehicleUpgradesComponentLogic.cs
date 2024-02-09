namespace RealmCore.Server.Modules.Vehicles;

internal sealed class VehicleUpgradesComponentLogic
{
    private readonly VehicleUpgradeCollection _vehicleUpgradesCollection;

    public VehicleUpgradesComponentLogic(IElementFactory elementFactory, VehicleUpgradeCollection vehicleUpgradesCollection)
    {
        _vehicleUpgradesCollection = vehicleUpgradesCollection;
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        if (element is RealmVehicle vehicle)
            vehicle.Upgrades.Rebuild += HandleRebuild;
    }

    private void HandleRebuild(IVehicleUpgradesFeature upgrades)
    {
        var vehicle = upgrades.Vehicle;

        var vehicleHandlingContext = new VehicleHandlingContext(vehicle.Model);

        if (upgrades.Count() != 0)
        {
            var vehicleHandlingModifiers = upgrades.Select(x => _vehicleUpgradesCollection.Get(x))
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

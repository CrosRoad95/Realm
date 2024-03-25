namespace RealmCore.Server.Modules.Vehicles.Tuning;

internal sealed class VehiclesTuningLogic
{
    private readonly VehicleUpgradesCollection _vehicleUpgradesCollection;

    public VehiclesTuningLogic(IElementFactory elementFactory, VehicleUpgradesCollection vehicleUpgradesCollection)
    {
        _vehicleUpgradesCollection = vehicleUpgradesCollection;
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is RealmVehicle vehicle)
            vehicle.Upgrades.Rebuild += HandleRebuild;
    }

    private void HandleRebuild(IVehicleUpgradesFeature upgrades)
    {
        var vehicle = upgrades.Vehicle;

        var vehicleHandlingContext = new VehicleHandlingContext(vehicle.Model);

        if (upgrades.Any())
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

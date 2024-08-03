namespace RealmCore.Server.Modules.Vehicles.Tuning;

internal sealed class VehiclesTuningHostedService : IHostedService
{
    private readonly IElementFactory _elementFactory;
    private readonly VehicleUpgradesCollection _vehicleUpgradesCollection;

    public VehiclesTuningHostedService(IElementFactory elementFactory, VehicleUpgradesCollection vehicleUpgradesCollection)
    {
        _elementFactory = elementFactory;
        _vehicleUpgradesCollection = vehicleUpgradesCollection;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _elementFactory.ElementCreated += HandleElementCreated;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _elementFactory.ElementCreated -= HandleElementCreated;
        return Task.CompletedTask;
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is RealmVehicle vehicle)
            vehicle.Upgrades.Rebuild += HandleRebuild;
    }

    private void HandleRebuild(VehicleUpgradesFeature upgrades)
    {
        var vehicle = upgrades.Vehicle;

        var vehicleHandlingContext = new VehicleHandlingContext(vehicle.Model);

        if (upgrades.Any())
        {
            var vehicleHandlingModifiers = upgrades.Select(x => _vehicleUpgradesCollection.Get(x))
                .Select(x => x.VehicleUpgrade)
                .ToArray();

            void Next(VehicleHandlingContext data, int index)
            {
                if (index < vehicleHandlingModifiers.Length)
                {
                    vehicleHandlingModifiers[index].Apply(data, newData => Next(newData, index + 1));
                }
            }

            Next(vehicleHandlingContext, 0);
        }

        vehicle.Handling = vehicleHandlingContext.VehicleHandling;
    }

}

namespace Realm.Server.Services;

internal class LoadService : ILoadService
{
    private readonly RepositoryFactory _repositoryFactory;
    private readonly IEntityFactory _entityFactory;
    private readonly ILogger _logger;

    public LoadService(RepositoryFactory repositoryFactory, IEntityFactory entityFactory, ILogger logger) {
        _repositoryFactory = repositoryFactory;
        _entityFactory = entityFactory;
        _logger = logger;
    }

    public async Task LoadAll()
    {
        await LoadAllVehicles();
    }

    private async Task LoadAllVehicles()
    {
        using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
        var results = await vehicleRepository
            .GetAll()
            .IncludeAll()
            .IsSpawned()
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();

        int i = 0;
        foreach (var vehicleData in results)
        {
            try
            {
                await Task.Delay(200);
                var entity = _entityFactory.CreateVehicle(vehicleData.Model, vehicleData.TransformAndMotion.Position, vehicleData.TransformAndMotion.Rotation, vehicleData.TransformAndMotion.Interior, vehicleData.TransformAndMotion.Dimension, $"vehicle {vehicleData.Id}",
                    entity =>
                    {
                        entity.AddComponent(new PrivateVehicleComponent(vehicleData));
                        entity.AddComponent(new VehicleUpgradesComponent(vehicleData.Upgrades));
                        entity.AddComponent(new MileageCounterComponent(vehicleData.Mileage));
                        foreach (var vehicleFuel in vehicleData.Fuels)
                            entity.AddComponent(new VehicleFuelComponent(vehicleFuel.FuelType, vehicleFuel.Amount, vehicleFuel.MaxCapacity, vehicleFuel.FuelConsumptionPerOneKm, vehicleFuel.MinimumDistanceThreshold)).Active = vehicleFuel.Active;
                    });
                i++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to spawn vehicle: {id}", vehicleData.Id);
            }
        }
        if (i > 0)
            _logger.LogInformation("Loaded: {amount} vehicles", i);
    }
}

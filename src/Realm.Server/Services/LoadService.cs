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
        // Load vehicles
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var results = await vehicleRepository
                .GetAll()
                .IncludeAll()
                //.Where(x => x.Spawned)
                .AsNoTrackingWithIdentityResolution()
                .ToListAsync();

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
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            _logger.Information("Loaded: {amount} vehicles", results.Count);

        }
    }
}

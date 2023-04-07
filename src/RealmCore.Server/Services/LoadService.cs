namespace RealmCore.Server.Services;

internal class LoadService : ILoadService
{
    private readonly RepositoryFactory _repositoryFactory;
    private readonly IEntityFactory _entityFactory;
    private readonly ILogger<LoadService> _logger;
    private readonly IVehiclesService _vehiclesService;

    public LoadService(RepositoryFactory repositoryFactory, IEntityFactory entityFactory, ILogger<LoadService> logger, IVehiclesService vehiclesService)
    {
        _repositoryFactory = repositoryFactory;
        _entityFactory = entityFactory;
        _logger = logger;
        _vehiclesService = vehiclesService;
    }

    public async Task LoadAll()
    {
        await LoadAllVehicles();
    }

    private async Task LoadAllVehicles()
    {
        using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
        var results = await vehicleRepository.GetAllSpawnedVehicles();

        int i = 0;
        foreach (var vehicleData in results)
        {
            try
            {
                //await Task.Delay(200);
                _vehiclesService.Spawn(vehicleData);
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

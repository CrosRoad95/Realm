using RealmCore.Persistance;

namespace RealmCore.Server.Services;

internal class LoadService : ILoadService
{
    private readonly RepositoryFactory _repositoryFactory;
    private readonly ILogger<LoadService> _logger;
    private readonly IVehiclesService _vehiclesService;

    public LoadService(RepositoryFactory repositoryFactory, ILogger<LoadService> logger, IVehiclesService vehiclesService)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
        _vehiclesService = vehiclesService;
    }

    public async Task LoadAll()
    {
        await LoadAllVehicles();
    }

    public async Task<Entity?> LoadVehicleById(int id)
    {
        using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
        var vehicleData = await vehicleRepository.GetVehicleById(id);
        if (vehicleData == null)
            throw new Exception($"Failed to load vehicle data of id {id}");

        try
        {
            return _vehiclesService.Spawn(vehicleData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to spawn vehicle: {id}", vehicleData.Id);
        }
        return null;
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

namespace RealmCore.Server.Services;

internal sealed class LoadService : ILoadService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ILogger<LoadService> _logger;
    private readonly IVehiclesService _vehiclesService;

    public LoadService(IVehicleRepository vehicleRepository, ILogger<LoadService> logger, IVehiclesService vehiclesService)
    {
        _vehicleRepository = vehicleRepository;
        _logger = logger;
        _vehiclesService = vehiclesService;
    }

    public async Task LoadAll()
    {
        await LoadAllVehicles();
    }

    public async Task<Entity> LoadVehicleById(int id)
    {
        var vehicleData = await _vehicleRepository.GetVehicleById(id).ConfigureAwait(false) ?? throw new Exception($"Failed to load vehicle data of id {id}");
        try
        {
            return _vehiclesService.Spawn(vehicleData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to spawn vehicle: {vehicleId}", vehicleData.Id);
            throw;
        }
    }

    private async Task LoadAllVehicles()
    {
        var results = await _vehicleRepository.GetAllSpawnedVehicles().ConfigureAwait(false);

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

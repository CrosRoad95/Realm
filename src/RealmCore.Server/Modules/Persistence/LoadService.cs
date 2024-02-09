namespace RealmCore.Server.Modules.Persistence;

public interface ILoadService
{
    Task LoadAll(CancellationToken cancellationToken = default);
    Task<RealmVehicle> LoadVehicleById(int id, CancellationToken cancellationToken = default);
}

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

    public async Task LoadAll(CancellationToken cancellationToken = default)
    {
        await LoadAllVehicles(cancellationToken);
    }

    public async Task<RealmVehicle> LoadVehicleById(int id, CancellationToken cancellationToken = default)
    {
        var vehicleData = await _vehicleRepository.GetVehicleById(id, cancellationToken) ?? throw new PersistantVehicleNotFoundException($"Failed to load vehicle data of id {id}");

        try
        {
            return await _vehiclesService.Spawn(vehicleData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to spawn vehicle: {vehicleId}", vehicleData.Id);
            throw;
        }
    }

    private async Task LoadAllVehicles(CancellationToken cancellationToken = default)
    {
        var results = await _vehicleRepository.GetAllSpawnedVehicles(cancellationToken);

        int i = 0;
        foreach (var vehicleData in results)
        {
            try
            {
                await _vehiclesService.Spawn(vehicleData, cancellationToken);
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

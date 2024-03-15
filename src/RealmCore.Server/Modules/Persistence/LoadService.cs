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
    private readonly IElementFactory _elementFactory;
    private readonly IVehiclesInUse _vehiclesInUse;

    public LoadService(IVehicleRepository vehicleRepository, ILogger<LoadService> logger, IElementFactory elementFactory, IVehiclesInUse vehiclesInUse)
    {
        _vehicleRepository = vehicleRepository;
        _logger = logger;
        _elementFactory = elementFactory;
        _vehiclesInUse = vehiclesInUse;
    }

    public async Task LoadAll(CancellationToken cancellationToken = default)
    {
        await LoadAllVehicles(cancellationToken);
    }

    public async Task<RealmVehicle> LoadVehicleById(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(LoadVehicleById));
        activity?.SetTag("Id", id);

        var vehicleId = id;
        if (vehicleId <= 0)
            throw new ArgumentOutOfRangeException(nameof(vehicleId));

        try
        {
            var vehicle = await _elementFactory.CreateVehicle(Location.Zero, VehicleModel.Perennial, async (vehicle) =>
            {
                var vehicleData = await vehicle.GetRequiredService<IVehicleRepository>().GetById(id, cancellationToken) ?? throw new PersistantVehicleNotFoundException($"Failed to load vehicle data of id {id}");

                if (vehicleData.IsRemoved)
                    throw new VehicleRemovedException(vehicleId);

                SetActive(vehicleId, vehicle);
                vehicle.Persistence.Load(vehicleData);

                await vehicle.GetRequiredService<IVehicleService>().SetVehicleSpawned(true, cancellationToken);

                vehicle.Upgrades.ForceRebuild();
            });

            activity?.SetStatus(ActivityStatusCode.Ok);
            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to spawn vehicle: {vehicleId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, ex.ToString());
            await _vehicleRepository.SetSpawned(id, false, cancellationToken);
            throw;
        }
    }

    private async Task LoadAllVehicles(CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(LoadAllVehicles));

        var results = await _vehicleRepository.GetAllSpawnedVehiclesIds(cancellationToken);

        int i = 0;
        foreach (var vehicleId in results)
        {
            try
            {
                await LoadVehicleById(vehicleId, cancellationToken);
                i++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to spawn vehicle: {id}", vehicleId);
            }
        }
        if (i > 0)
            _logger.LogInformation("Loaded: {amount} vehicles", i);
    }

    private void SetActive(int vehicleId, RealmVehicle vehicle)
    {
        if (!_vehiclesInUse.TrySetActive(vehicleId, vehicle))
            throw new PersistantVehicleAlreadySpawnedException("Failed to create already existing vehicle.");
    }

    public static readonly ActivitySource Activity = new("RealmCore.LoadService", "1.0.0");
}

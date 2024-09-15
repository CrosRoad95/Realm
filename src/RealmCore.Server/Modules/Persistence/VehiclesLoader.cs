namespace RealmCore.Server.Modules.Persistence;

public sealed class VehiclesLoader
{
    private readonly VehicleRepository _vehicleRepository;
    private readonly ILogger<VehiclesLoader> _logger;
    private readonly IElementFactory _elementFactory;
    private readonly VehiclesInUse _vehiclesInUse;

    public VehiclesLoader(VehicleRepository vehicleRepository, ILogger<VehiclesLoader> logger, IElementFactory elementFactory, VehiclesInUse vehiclesInUse)
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
                var vehicleData = await vehicle.GetRequiredService<VehicleRepository>().GetById(id, cancellationToken) ?? throw new Exception("VehicleData not found");

                if (vehicleData.IsRemoved)
                     throw new Exception("Vehicle removed");

                if (!_vehiclesInUse.TrySetActive(vehicleId, vehicle))
                    throw new Exception("Failed to create already existing vehicle.");

                vehicle.Persistence.Load(vehicleData);

                await vehicle.GetRequiredService<VehicleService>().SetVehicleSpawned(true, cancellationToken);

                vehicle.Upgrades.ForceRebuild();

                vehicle.AccessController = VehiclePrivateAccessController.Instance;
            });

            activity?.SetStatus(ActivityStatusCode.Ok);
            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to spawn vehicle: {vehicleId}", id);
            activity?.SetStatus(ActivityStatusCode.Error, ex.ToString());
            try
            {
                await _vehicleRepository.TrySetSpawned(id, false, cancellationToken);
            }
            catch(Exception)
            {
                // Ignore,
            }
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

    public static readonly ActivitySource Activity = new("RealmCore.LoadService", "1.0.0");
}

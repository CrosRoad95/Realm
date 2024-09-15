namespace RealmCore.Server.Modules.Vehicles;

public sealed class VehiclesService
{
    private readonly IElementFactory _elementFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly UsersInUse _activeUsers;
    private readonly VehiclesInUse _vehiclesInUse;
    private readonly ILogger<VehiclesService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public VehiclesService(IElementFactory elementFactory, IDateTimeProvider dateTimeProvider, UsersInUse activeUsers, VehiclesInUse vehiclesInUse, ILogger<VehiclesService> logger, IServiceProvider serviceProvider)
    {
        _elementFactory = elementFactory;
        _dateTimeProvider = dateTimeProvider;
        _activeUsers = activeUsers;
        _vehiclesInUse = vehiclesInUse;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<RealmVehicle?> CreatePersistantVehicle(Location location, VehicleModel model, CancellationToken cancellationToken = default)
    {
        var vehicle = _elementFactory.CreateVehicle(location, model);
        var vehicleData = await vehicle.GetRequiredService<VehicleRepository>().CreateVehicle((ushort)model, _dateTimeProvider.Now, cancellationToken);

        if (_vehiclesInUse.TrySetActive(vehicleData.Id, vehicle))
        {
            vehicle.Persistence.Load(vehicleData, true);
            return vehicle;
        }
        else
        {
            vehicle.Destroy();
            return null;
        }
    }

    public async Task<VehicleData[]> GetAllVehicles(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (player.User.IsLoggedIn)
        {
            var vehicleRepository = player.GetRequiredService<VehicleRepository>();
            return await vehicleRepository.GetVehiclesByUserId(player.UserId, null, cancellationToken);
        }
        return [];
    }

    public async Task<LightInfoVehicleDto[]> GetAllLightVehicles(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (player.User.IsLoggedIn)
        {
            var vehicleRepository = player.GetRequiredService<VehicleRepository>();
            return await vehicleRepository.GetLightVehiclesByUserId(player.UserId, cancellationToken);
        }
        return [];
    }

    public IEnumerable<RealmPlayer> GetOnlineOwners(RealmVehicle vehicle)
    {
        foreach (var owner in vehicle.Access.Owners)
        {
            if (_activeUsers.TryGetPlayerByUserId(owner.UserId, out var player) && player != null)
                yield return player;
        }
    }

    public Task Destroy(RealmVehicle vehicle)
    {
        return vehicle.GetRequiredService<VehicleService>().Destroy();
    }

    public async Task<bool> Save(RealmVehicle vehicle)
    {
        if (!vehicle.Persistence.IsLoaded)
            return false;

        return await vehicle.GetRequiredService<ElementSaveService>().Save();
    }

    public async Task<RealmVehicle?> ConvertToPersistantVehicle(RealmVehicle vehicle, CancellationToken cancellationToken = default)
    {
        if (vehicle.Persistence.IsLoaded)
            throw new InvalidOperationException();

        var vehicleRepository = vehicle.GetRequiredService<VehicleRepository>();
        var vehicleService = vehicle.GetRequiredService<VehicleService>();
        var location = vehicle.GetLocation();

        var vehicleData = await vehicleRepository.CreateVehicle(vehicle.Model, _dateTimeProvider.Now, cancellationToken);
        if (!_vehiclesInUse.TrySetActive(vehicleData.Id, vehicle))
        {
            return null;
        }

        vehicle.Persistence.Load(vehicleData, true);

        var occupants = vehicle.Occupants.ToArray();
        await vehicleService.Destroy(cancellationToken);

        using var scope = _serviceProvider.CreateScope();
        var vehicleLoader = scope.ServiceProvider.GetRequiredService<VehiclesLoader>();
        var persistentVehicle = await vehicleLoader.LoadVehicleById(vehicleData.Id, cancellationToken);
        persistentVehicle.SetLocation(location);
        foreach (var pair in occupants)
            persistentVehicle.AddPassenger(pair.Key, pair.Value);

        return persistentVehicle;
    }
}

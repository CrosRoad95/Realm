namespace RealmCore.Server.Modules.Vehicles;

public interface IVehiclesService
{
    Task<RealmVehicle?> ConvertToPersistantVehicle(RealmVehicle vehicle, CancellationToken cancellationToken = default);
    Task<RealmVehicle?> CreatePersistantVehicle(Location location, VehicleModel model, CancellationToken cancellationToken = default);
    Task Destroy(RealmVehicle vehicle);
    Task<LightInfoVehicleDto[]> GetAllLightVehicles(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<VehicleData[]> GetAllVehicles(RealmPlayer player, CancellationToken cancellationToken = default);
    IEnumerable<RealmPlayer> GetOnlineOwners(RealmVehicle vehicle);
    Task<bool> Save(RealmVehicle vehicle);
}

internal sealed class VehiclesService : IVehiclesService
{
    private readonly IElementFactory _elementFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUsersInUse _activeUsers;
    private readonly IVehiclesInUse _vehiclesInUse;
    private readonly ILogger<VehiclesService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public VehiclesService(IElementFactory elementFactory, IDateTimeProvider dateTimeProvider, IUsersInUse activeUsers, IVehiclesInUse vehiclesInUse, ILogger<VehiclesService> logger, IServiceProvider serviceProvider)
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
        var vehicleData = await vehicle.GetRequiredService<IVehicleRepository>().CreateVehicle((ushort)model, _dateTimeProvider.Now, cancellationToken);

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
            var vehicleRepository = player.GetRequiredService<IVehicleRepository>();
            return await vehicleRepository.GetVehiclesByUserId(player.UserId, null, cancellationToken);
        }
        return [];
    }

    public async Task<LightInfoVehicleDto[]> GetAllLightVehicles(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (player.User.IsLoggedIn)
        {
            var vehicleRepository = player.GetRequiredService<IVehicleRepository>();
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
        return vehicle.GetRequiredService<IVehicleService>().Destroy();
    }

    public async Task<bool> Save(RealmVehicle vehicle)
    {
        if (!vehicle.Persistence.IsLoaded)
            return false;

        return await vehicle.GetRequiredService<IElementSaveService>().Save();
    }

    public async Task<RealmVehicle?> ConvertToPersistantVehicle(RealmVehicle vehicle, CancellationToken cancellationToken = default)
    {
        if (vehicle.Persistence.IsLoaded)
            throw new InvalidOperationException();

        var vehicleRepository = vehicle.GetRequiredService<IVehicleRepository>();
        var vehicleService = vehicle.GetRequiredService<IVehicleService>();
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
        var vehicleLoader = scope.ServiceProvider.GetRequiredService<IVehicleLoader>();
        var persistentVehicle = await vehicleLoader.LoadVehicleById(vehicleData.Id, cancellationToken);
        persistentVehicle.SetLocation(location);
        foreach (var pair in occupants)
            persistentVehicle.AddPassenger(pair.Key, pair.Value);

        return persistentVehicle;
    }
}

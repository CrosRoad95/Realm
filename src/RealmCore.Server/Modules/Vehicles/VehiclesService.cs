namespace RealmCore.Server.Modules.Vehicles;

public interface IVehiclesService
{
    Task<RealmVehicle> CreatePersistantVehicle(Location location, VehicleModel model, CancellationToken cancellationToken = default);
    Task Destroy(RealmVehicle vehicle);
    Task<List<LightInfoVehicleDto>> GetAllLightVehicles(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetAllSpawnedVehicles(CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetAllVehicles(RealmPlayer player, CancellationToken cancellationToken = default);
    IEnumerable<RealmPlayer> GetOnlineOwners(RealmVehicle vehicle);
    Task Save(RealmVehicle vehicle);
    Task<RealmVehicle> Spawn(VehicleData vehicleData, CancellationToken cancellationToken = default);
}

internal sealed class VehiclesService : IVehiclesService
{
    private readonly RealmVehicle _vehicle;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IElementFactory _elementFactory;
    private readonly ISaveService _saveService;
    private readonly ItemsCollection _itemsCollection;
    private readonly IVehicleEventRepository _vehicleEventRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly VehicleUpgradesCollection _vehicleUpgradesCollection;
    private readonly VehicleEnginesCollection _vehicleEnginesCollection;
    private readonly IUsersInUse _activeUsers;
    private readonly IVehiclesInUse _vehiclesInUse;
    private readonly ILogger<VehicleService> _logger;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public VehiclesService(VehicleContext vehicleContext, IVehicleRepository vehicleRepository, IElementFactory elementFactory, ISaveService saveService, ItemsCollection itemsCollection, IVehicleEventRepository vehicleEventRepository, IDateTimeProvider dateTimeProvider, VehicleUpgradesCollection vehicleUpgradeCollection, VehicleEnginesCollection vehicleEnginesCollection, IUsersInUse activeUsers, IVehiclesInUse vehiclesInUse, ILogger<VehicleService> logger)
    {
        _vehicle = vehicleContext.Vehicle;
        _vehicleRepository = vehicleRepository;
        _elementFactory = elementFactory;
        _saveService = saveService;
        _itemsCollection = itemsCollection;
        _vehicleEventRepository = vehicleEventRepository;
        _dateTimeProvider = dateTimeProvider;
        _vehicleUpgradesCollection = vehicleUpgradeCollection;
        _vehicleEnginesCollection = vehicleEnginesCollection;
        _activeUsers = activeUsers;
        _vehiclesInUse = vehiclesInUse;
        _logger = logger;
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { DoubleConverter.Instance }
        };
    }

    public async Task<RealmVehicle> CreatePersistantVehicle(Location location, VehicleModel model, CancellationToken cancellationToken = default)
    {
        var vehicle = _elementFactory.CreateVehicle(location, model);
        var vehicleData = await vehicle.GetRequiredService<IVehicleRepository>().CreateVehicle((ushort)model, _dateTimeProvider.Now, cancellationToken);
        try
        {
            SetActive(vehicleData.Id, vehicle);
            vehicle.Persistence.Load(vehicleData);
            return vehicle;
        }
        catch (Exception)
        {
            vehicle.Destroy();
            throw;
        }
    }

    public async Task<List<VehicleData>> GetAllVehicles(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (player.User.IsSignedIn)
        {
            return await _vehicleRepository.GetVehiclesByUserId(player.PersistentId, null, cancellationToken);
        }
        return [];
    }

    public Task<List<VehicleData>> GetAllSpawnedVehicles(CancellationToken cancellationToken = default)
    {
        return _vehicleRepository.GetAllSpawnedVehicles(cancellationToken);
    }

    public async Task<List<LightInfoVehicleDto>> GetAllLightVehicles(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (player.User.IsSignedIn)
        {
            return await _vehicleRepository.GetLightVehiclesByUserId(player.PersistentId, cancellationToken);
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

    public async Task Destroy(RealmVehicle vehicle)
    {
        if (vehicle.Persistence.IsLoaded)
        {
            await vehicle.GetRequiredService<ISaveService>().Save(vehicle);
            var id = vehicle.Persistence.Id;
            await vehicle.GetRequiredService<ISaveService>().Save(vehicle);
            await _vehicleRepository.SetSpawned(id, false);
            _vehiclesInUse.TrySetInactive(id);
        }

        vehicle.Destroy();
    }

    public async Task Save(RealmVehicle vehicle)
    {
        if (!vehicle.Persistence.IsLoaded)
            throw new PersistantVehicleNotLoadedException();

        await vehicle.GetRequiredService<ISaveService>().Save(vehicle);
    }

    public async Task<RealmVehicle> Spawn(VehicleData vehicleData, CancellationToken cancellationToken = default)
    {
        var vehicleId = vehicleData.Id;
        if (vehicleId <= 0)
            throw new ArgumentOutOfRangeException(nameof(vehicleId));

        if (vehicleData.IsRemoved)
            throw new VehicleRemovedException(vehicleId);

        var location = new Location(vehicleData.TransformAndMotion.Position, vehicleData.TransformAndMotion.Rotation, vehicleData.TransformAndMotion.Interior, vehicleData.TransformAndMotion.Dimension);
        var vehicle = _elementFactory.CreateVehicle(location, (VehicleModel)vehicleData.Model,
            vehicle =>
            {
                SetActive(vehicleId, vehicle);
                vehicle.Persistence.Load(vehicleData);
            });

        await vehicle.GetRequiredService<IVehicleService>().SetVehicleSpawned(true, cancellationToken);
        vehicle.Upgrades.ForceRebuild();
        return vehicle;
    }

    private void SetActive(int vehicleId, RealmVehicle vehicle)
    {
        if (!_vehiclesInUse.TrySetActive(vehicleId, vehicle))
            throw new PersistantVehicleAlreadySpawnedException("Failed to create already existing vehicle.");
    }
}

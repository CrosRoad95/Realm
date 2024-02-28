namespace RealmCore.Server.Modules.Vehicles;

public interface IVehiclesService
{
    Task<RealmVehicle> ConvertToPersistantVehicle(RealmVehicle vehicle, CancellationToken cancellationToken = default);
    Task<RealmVehicle> CreatePersistantVehicle(Location location, VehicleModel model, CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetAllSpawnedVehicles(CancellationToken cancellationToken = default);
    Task<List<LightInfoVehicleDto>> GetAllLightVehicles(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetAllVehicles(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<bool> SetVehicleSpawned(RealmVehicle vehicle, bool spawned = true, CancellationToken cancellationToken = default);
    Task<RealmVehicle> Spawn(VehicleData vehicleData, CancellationToken cancellationToken = default);
    IEnumerable<RealmPlayer> GetOnlineOwners(RealmVehicle vehicle);
    Task Destroy(RealmVehicle vehicle);
}

internal sealed class VehiclesService : IVehiclesService
{
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
    private readonly ILogger<VehiclesService> _logger;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public VehiclesService(IVehicleRepository vehicleRepository, IElementFactory elementFactory, ISaveService saveService, ItemsCollection itemsCollection, IVehicleEventRepository vehicleEventRepository, IDateTimeProvider dateTimeProvider, VehicleUpgradesCollection vehicleUpgradeCollection, VehicleEnginesCollection vehicleEnginesCollection, IUsersInUse activeUsers, IVehiclesInUse vehiclesInUse, ILogger<VehiclesService> logger)
    {
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

    public async Task<RealmVehicle> ConvertToPersistantVehicle(RealmVehicle vehicle, CancellationToken cancellationToken = default)
    {
        if (vehicle.Persistence.IsLoaded)
            throw new InvalidOperationException();

        var vehicleData = await _vehicleRepository.CreateVehicle(vehicle.Model, _dateTimeProvider.Now, cancellationToken);

        SetActive(vehicleData.Id, vehicle);
        vehicle.Persistence.Load(vehicleData);
        return vehicle;
    }

    public async Task<List<LightInfoVehicleDto>> GetAllLightVehicles(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (player.User.IsSignedIn)
        {
            return await _vehicleRepository.GetLightVehiclesByUserId(player.PersistentId, cancellationToken);
        }
        return [];
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

    public async Task<bool> SetVehicleSpawned(RealmVehicle vehicle, bool spawned = true, CancellationToken cancellationToken = default)
    {
        return await _vehicleRepository.SetSpawned(vehicle.Persistence.Id, spawned, cancellationToken);
    }

    private void SetActive(int vehicleId, RealmVehicle vehicle)
    {
        if (!_vehiclesInUse.TrySetActive(vehicleId, vehicle))
            throw new PersistantVehicleAlreadySpawnedException("Failed to create already existing vehicle.");
    }

    public async Task<RealmVehicle> Spawn(VehicleData vehicleData, CancellationToken cancellationToken = default)
    {
        var vehicleId = vehicleData.Id;
        if(vehicleId <= 0)
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

        await SetVehicleSpawned(vehicle, true, cancellationToken);
        vehicle.Upgrades.ForceRebuild();
        return vehicle;
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
        if(vehicle.Persistence.IsLoaded)
        {
            await _saveService.Save(vehicle);
            var id = vehicle.Persistence.Id;
            await _vehicleRepository.SetSpawned(id, false);
            _vehiclesInUse.TrySetInactive(id);
        }

        vehicle.Destroy();
    }
}

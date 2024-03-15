using SlipeServer.Server.Elements;

namespace RealmCore.Server.Modules.Vehicles;

public interface IVehiclesService
{
    Task<RealmVehicle> ConvertToPersistantVehicle(RealmVehicle vehicle, CancellationToken cancellationToken = default);
    Task<RealmVehicle> CreatePersistantVehicle(Location location, VehicleModel model, CancellationToken cancellationToken = default);
    Task Destroy(RealmVehicle vehicle);
    Task<List<LightInfoVehicleDto>> GetAllLightVehicles(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetAllVehicles(RealmPlayer player, CancellationToken cancellationToken = default);
    IEnumerable<RealmPlayer> GetOnlineOwners(RealmVehicle vehicle);
    Task Save(RealmVehicle vehicle);
}

internal sealed class VehiclesService : IVehiclesService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IElementFactory _elementFactory;
    private readonly ItemsCollection _itemsCollection;
    private readonly IVehicleEventRepository _vehicleEventRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly VehicleUpgradesCollection _vehicleUpgradesCollection;
    private readonly VehicleEnginesCollection _vehicleEnginesCollection;
    private readonly IUsersInUse _activeUsers;
    private readonly IVehiclesInUse _vehiclesInUse;
    private readonly ILogger<VehicleService> _logger;
    private readonly ILoadService _loadService;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public VehiclesService(IVehicleRepository vehicleRepository, IElementFactory elementFactory, ItemsCollection itemsCollection, IVehicleEventRepository vehicleEventRepository, IDateTimeProvider dateTimeProvider, VehicleUpgradesCollection vehicleUpgradeCollection, VehicleEnginesCollection vehicleEnginesCollection, IUsersInUse activeUsers, IVehiclesInUse vehiclesInUse, ILogger<VehicleService> logger, ILoadService loadService)
    {
        _vehicleRepository = vehicleRepository;
        _elementFactory = elementFactory;
        _itemsCollection = itemsCollection;
        _vehicleEventRepository = vehicleEventRepository;
        _dateTimeProvider = dateTimeProvider;
        _vehicleUpgradesCollection = vehicleUpgradeCollection;
        _vehicleEnginesCollection = vehicleEnginesCollection;
        _activeUsers = activeUsers;
        _vehiclesInUse = vehiclesInUse;
        _logger = logger;
        _loadService = loadService;
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

    public Task Destroy(RealmVehicle vehicle)
    {
        return vehicle.GetRequiredService<IVehicleService>().Destroy();
    }

    public async Task Save(RealmVehicle vehicle)
    {
        if (!vehicle.Persistence.IsLoaded)
            throw new PersistantVehicleNotLoadedException();

        await vehicle.GetRequiredService<ISaveService>().Save();
    }

    public async Task<RealmVehicle> ConvertToPersistantVehicle(RealmVehicle vehicle, CancellationToken cancellationToken = default)
    {
        if (vehicle.Persistence.IsLoaded)
            throw new InvalidOperationException();

        var vehicleRepository = vehicle.GetRequiredService<IVehicleRepository>();
        var vehicleService = vehicle.GetRequiredService<IVehicleService>();
        var location = vehicle.GetLocation();

        var vehicleData = await vehicleRepository.CreateVehicle(vehicle.Model, _dateTimeProvider.Now, cancellationToken);
        SetActive(vehicleData.Id, vehicle);
        vehicle.Persistence.Load(vehicleData, true);

        var occupants = vehicle.Occupants.ToList();
        await vehicleService.Destroy(cancellationToken);
        var persistentVehicle = await _loadService.LoadVehicleById(vehicleData.Id, cancellationToken);
        persistentVehicle.SetLocation(location);
        foreach (var pair in occupants)
            persistentVehicle.AddPassenger(pair.Key, pair.Value);

        return persistentVehicle;
    }

    private void SetActive(int vehicleId, RealmVehicle vehicle)
    {
        if (!_vehiclesInUse.TrySetActive(vehicleId, vehicle))
            throw new PersistantVehicleAlreadySpawnedException("Failed to create already existing vehicle.");
    }
}

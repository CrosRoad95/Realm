using RealmCore.Server.Json.Converters;

namespace RealmCore.Server.Services;

internal sealed class VehiclesService : IVehiclesService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IElementFactory _elementFactory;
    private readonly ISaveService _saveService;
    private readonly ItemsRegistry _itemsRegistry;
    private readonly IVehicleEventRepository _vehicleEventRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly VehicleUpgradeRegistry _vehicleUpgradeRegistry;
    private readonly VehicleEnginesRegistry _vehicleEnginesRegistry;
    private readonly IActiveUsers _activeUsers;
    private readonly IActiveVehicles _activeVehicles;
    private readonly ILogger<VehiclesService> _logger;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public VehiclesService(IVehicleRepository vehicleRepository, IElementFactory elementFactory, ISaveService saveService, ItemsRegistry itemsRegistry, IVehicleEventRepository vehicleEventRepository, IDateTimeProvider dateTimeProvider, VehicleUpgradeRegistry vehicleUpgradeRegistry, VehicleEnginesRegistry vehicleEnginesRegistry, IActiveUsers activeUsers, IActiveVehicles activeVehicles, ILogger<VehiclesService> logger)
    {
        _vehicleRepository = vehicleRepository;
        _elementFactory = elementFactory;
        _saveService = saveService;
        _itemsRegistry = itemsRegistry;
        _vehicleEventRepository = vehicleEventRepository;
        _dateTimeProvider = dateTimeProvider;
        _vehicleUpgradeRegistry = vehicleUpgradeRegistry;
        _vehicleEnginesRegistry = vehicleEnginesRegistry;
        _activeUsers = activeUsers;
        _activeVehicles = activeVehicles;
        _logger = logger;
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { DoubleConverter.Instance }
        };
        _elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        if (element is not RealmVehicle vehicle)
            return;

        vehicle.Destroyed += HandleDestroyed;
    }

    private async void HandleDestroyed(Element element)
    {
        try
        {
            var vehicle = (RealmVehicle)element;
            var id = vehicle.Persistance.Id;
            _activeVehicles.TrySetInactive(id);
            await _vehicleRepository.SetSpawned(id, false);
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }

    public async Task<RealmVehicle> CreatePersistantVehicle(ushort model, Vector3 position, Vector3 rotation, CancellationToken cancellationToken = default)
    {
        var vehicle = _elementFactory.CreateVehicle(model, position, rotation);
        var vehicleData = await vehicle.GetRequiredService<IVehicleRepository>().CreateVehicle(model, _dateTimeProvider.Now, cancellationToken);
        try
        {
            TrySetActive(vehicleData.Id, vehicle);
            vehicle.Persistance.Load(vehicleData);
            return vehicle;
        }
        catch(Exception)
        {
            vehicle.Destroy();
            throw;
        }
    }

    public async Task<RealmVehicle> ConvertToPersistantVehicle(RealmVehicle vehicle, CancellationToken cancellationToken = default)
    {
        if (vehicle.Persistance.IsLoaded)
            throw new InvalidOperationException();

        var vehicleData = await _vehicleRepository.CreateVehicle(vehicle.Model, _dateTimeProvider.Now, cancellationToken);

        TrySetActive(vehicleData.Id, vehicle);
        vehicle.Persistance.Load(vehicleData);
        return vehicle;
    }

    public async Task<List<LightInfoVehicleDTO>> GetAllLightVehicles(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (player.IsSignedIn)
        {
            return await _vehicleRepository.GetLightVehiclesByUserId(player.UserId, cancellationToken);
        }
        return new();
    }

    public async Task<List<VehicleData>> GetAllVehicles(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if(player.IsSignedIn)
        {
            return await _vehicleRepository.GetVehiclesByUserId(player.UserId, null, cancellationToken);
        }
        return new();
    }

    public Task<List<VehicleData>> GetAllSpawnedVehicles(CancellationToken cancellationToken = default)
    {
        return _vehicleRepository.GetAllSpawnedVehicles(cancellationToken);
    }

    public async Task<bool> SetVehicleSpawned(RealmVehicle vehicle, bool spawned = true, CancellationToken cancellationToken = default)
    {
        return await _vehicleRepository.SetSpawned(vehicle.Persistance.Id, spawned, cancellationToken);
    }

    private void TrySetActive(int vehicleId, RealmVehicle vehicle)
    {
        if (!_activeVehicles.TrySetActive(vehicleId, vehicle))
            throw new PersistantVehicleAlreadySpawnedException("Failed to create already existing vehicle.");
    }

    public async Task<RealmVehicle> Spawn(VehicleData vehicleData, CancellationToken cancellationToken = default)
    {
        if(vehicleData.IsRemoved)
            throw new VehicleRemovedException(vehicleData.Id);

        var vehicle = _elementFactory.CreateVehicle(vehicleData.Model, vehicleData.TransformAndMotion.Position, vehicleData.TransformAndMotion.Rotation, vehicleData.TransformAndMotion.Interior, vehicleData.TransformAndMotion.Dimension,
            vehicle =>
            {
                TrySetActive(vehicleData.Id, vehicle);
                var components = vehicle.Components;
                vehicle.Persistance.Load(vehicleData);

                if (vehicleData.Inventories != null && vehicleData.Inventories.Count != 0)
                {
                    foreach (var inventory in vehicleData.Inventories)
                    {
                        var items = inventory.InventoryItems
                            .Select(x =>
                                new Item(_itemsRegistry, x.ItemId, x.Number, JsonConvert.DeserializeObject<Metadata>(x.MetaData, _jsonSerializerSettings))
                            )
                            .ToList();
                        components.AddComponent(new InventoryComponent(inventory.Size, inventory.Id, items));
                    }
                }

                return []; // TODO:
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
}

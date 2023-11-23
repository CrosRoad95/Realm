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
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public VehiclesService(IVehicleRepository vehicleRepository, IElementFactory elementFactory, ISaveService saveService, ItemsRegistry itemsRegistry, IVehicleEventRepository vehicleEventRepository, IDateTimeProvider dateTimeProvider, VehicleUpgradeRegistry vehicleUpgradeRegistry, VehicleEnginesRegistry vehicleEnginesRegistry, IActiveUsers activeUsers, IActiveVehicles activeVehicles)
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
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { DoubleConverter.Instance }
        };
    }

    public async Task<RealmVehicle> CreatePersistantVehicle(ushort model, Vector3 position, Vector3 rotation)
    {
        var vehicleData = await _vehicleRepository.CreateVehicle(model, _dateTimeProvider.Now);
        var vehicle = _elementFactory.CreateVehicle(model, position, rotation);
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

    public async Task<RealmVehicle> ConvertToPersistantVehicle(RealmVehicle vehicle)
    {
        if (vehicle.Persistance.IsLoaded)
            throw new InvalidOperationException();

        var vehicleData = await _vehicleRepository.CreateVehicle(vehicle.Model, _dateTimeProvider.Now);

        TrySetActive(vehicleData.Id, vehicle);
        vehicle.Persistance.Load(vehicleData);
        return vehicle;
    }

    public async Task<List<LightInfoVehicleDTO>> GetAllLightVehicles(RealmPlayer player)
    {
        if (player.IsSignedIn)
        {
            return await _vehicleRepository.GetLightVehiclesByUserId(player.UserId);
        }
        return new();
    }

    public async Task<List<VehicleData>> GetAllVehicles(RealmPlayer player)
    {
        if(player.IsSignedIn)
        {
            return await _vehicleRepository.GetVehiclesByUserId(player.UserId);
        }
        return new();
    }

    public Task<List<VehicleData>> GetAllSpawnedVehicles()
    {
        return _vehicleRepository.GetAllSpawnedVehicles();
    }

    public async Task Destroy(RealmVehicle vehicle)
    {
        await _vehicleRepository.SetSpawned(vehicle.Persistance.Id, false);
        await _saveService.BeginSave(vehicle);
        await _saveService.Commit();
        vehicle.Destroy();
    }

    public async Task<bool> SetVehicleSpawned(RealmVehicle vehicle, bool spawned = true)
    {
        return await _vehicleRepository.SetSpawned(vehicle.Persistance.Id, spawned);
    }

    private void TrySetActive(int vehicleId, RealmVehicle vehicle)
    {
        if (!_activeVehicles.TrySetActive(vehicleId, vehicle))
            throw new Exception("Failed to create already existing vehicle.");
    }

    public async Task<RealmVehicle> Spawn(VehicleData vehicleData)
    {
        if(vehicleData.IsRemoved)
            throw new VehicleRemovedException(vehicleData.Id);

        var vehicle = _elementFactory.CreateVehicle(vehicleData.Model, vehicleData.TransformAndMotion.Position, vehicleData.TransformAndMotion.Rotation, vehicleData.TransformAndMotion.Interior, vehicleData.TransformAndMotion.Dimension,
            vehicle =>
            {
                TrySetActive(vehicleData.Id, vehicle);
                var components = vehicle.Components;
                vehicle.Persistance.Load(vehicleData);
                if (vehicleData.VehicleEngines.Count != 0)
                    components.AddComponent(new VehicleEngineComponent(vehicleData.VehicleEngines));
                else
                    components.AddComponent<VehicleEngineComponent>();

                if (vehicleData.Fuels.Count != 0)
                {
                    foreach (var vehicleFuel in vehicleData.Fuels)
                        components.AddComponent(new FuelComponent(vehicleFuel.FuelType, vehicleFuel.Amount, vehicleFuel.MaxCapacity, vehicleFuel.FuelConsumptionPerOneKm, vehicleFuel.MinimumDistanceThreshold)).Active = vehicleFuel.Active;
                }

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

        await SetVehicleSpawned(vehicle);
        return vehicle;
    }

    public IEnumerable<RealmPlayer> GetOnlineOwner(RealmVehicle vehicle)
    {
        foreach (var owner in vehicle.Access.Owners)
        {
            if (_activeUsers.TryGetPlayerByUserId(owner.UserId, out var player) && player != null)
                yield return player;
        }
    }
}

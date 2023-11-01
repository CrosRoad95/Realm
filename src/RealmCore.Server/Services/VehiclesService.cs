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

    public VehiclesService(IVehicleRepository vehicleRepository, IElementFactory entityFactory, ISaveService saveService, ItemsRegistry itemsRegistry, IVehicleEventRepository vehicleEventRepository, IDateTimeProvider dateTimeProvider, VehicleUpgradeRegistry vehicleUpgradeRegistry, VehicleEnginesRegistry vehicleEnginesRegistry, IActiveUsers activeUsers, IActiveVehicles activeVehicles)
    {
        _vehicleRepository = vehicleRepository;
        _elementFactory = entityFactory;
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

    public async Task<RealmVehicle> CreateVehicle(ushort model, Vector3 position, Vector3 rotation)
    {
        var vehicleData = await _vehicleRepository.CreateVehicle(model, _dateTimeProvider.Now);
        return _elementFactory.CreateVehicle(model, position, rotation, elementBuilder: vehicle =>
        {
            vehicle.Components.AddComponent(new PrivateVehicleComponent(vehicleData, _dateTimeProvider));
        });
    }

    public async Task<RealmVehicle> ConvertToPrivateVehicle(RealmVehicle vehicle)
    {
        if (vehicle.Components.HasComponent<PrivateVehicleComponent>())
            throw new InvalidOperationException();

        var vehicleData = await _vehicleRepository.CreateVehicle(vehicle.Model, _dateTimeProvider.Now);
        vehicle.Components.AddComponent(new PrivateVehicleComponent(vehicleData, _dateTimeProvider));
        return vehicle;
    }

    public async Task<List<LightInfoVehicleDTO>> GetAllLightVehicles(RealmPlayer player)
    {
        if (player.Components.TryGetComponent(out UserComponent userComponent))
        {
            return await _vehicleRepository.GetLightVehiclesByUserId(userComponent.Id);
        }
        return new();
    }

    public async Task<List<VehicleData>> GetAllVehicles(RealmPlayer player)
    {
        if(player.Components.TryGetComponent(out UserComponent userComponent))
        {
            return await _vehicleRepository.GetVehiclesByUserId(userComponent.Id);
        }
        return new();
    }

    public Task<List<VehicleData>> GetAllSpawnedVehicles()
    {
        return _vehicleRepository.GetAllSpawnedVehicles();
    }

    public async Task Destroy(RealmVehicle vehicle)
    {
        await _vehicleRepository.SetSpawned(vehicle.Components.GetRequiredComponent<PrivateVehicleComponent>().Id, false);
        await _saveService.BeginSave(vehicle);
        await _saveService.Commit();
        vehicle.Destroy();
    }

    public async Task<bool> SetVehicleSpawned(RealmVehicle vehicle, bool spawned = true)
    {
        if (vehicle.Components.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            return await _vehicleRepository.SetSpawned(privateVehicleComponent.Id, spawned);
        }
        return false;
    }

    public async Task<VehicleAccess?> GetVehicleAccess(RealmVehicle vehicle)
    {
        if (vehicle.Components.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            var vehiclesAccesses = await _vehicleRepository.GetAllVehicleAccesses(privateVehicleComponent.Id);
            return new VehicleAccess(vehiclesAccesses);
        }
        return null;
    }

    public async Task<RealmVehicle> Spawn(VehicleData vehicleData)
    {
        if(vehicleData.IsRemoved)
            throw new VehicleRemovedException(vehicleData.Id);

        var vehicleEntity = _elementFactory.CreateVehicle(vehicleData.Model, vehicleData.TransformAndMotion.Position, vehicleData.TransformAndMotion.Rotation, vehicleData.TransformAndMotion.Interior, vehicleData.TransformAndMotion.Dimension,
            vehicle =>
            {
                if (!_activeVehicles.TrySetActive(vehicleData.Id, vehicle))
                    throw new Exception("Failed to create already existing vehicle.");

                var components = vehicle.Components;
                components.AddComponent(new PrivateVehicleComponent(vehicleData, _dateTimeProvider));
                components.AddComponent(new VehicleUpgradesComponent(vehicleData.Upgrades));
                components.AddComponent(new MileageCounterComponent(vehicleData.Mileage));
                if (vehicleData.VehicleEngines.Count != 0)
                    components.AddComponent(new VehicleEngineComponent(vehicleData.VehicleEngines));
                else
                    components.AddComponent<VehicleEngineComponent>();
                components.AddComponent(new VehiclePartDamageComponent(vehicleData.PartDamages));

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

            });

        await SetVehicleSpawned(vehicleEntity);
        return vehicleEntity;
    }

    public async Task<bool> AddVehicleEvent(RealmVehicle vehicle, int eventId, string? metadata = null)
    {
        if(vehicle.Components.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            await _vehicleEventRepository.AddEvent(privateVehicleComponent.Id, eventId, _dateTimeProvider.Now);
            return true;
        }
        return false;
    }

    public async Task<List<VehicleEventData>> GetAllVehicleEvents(RealmVehicle vehicle, IEnumerable<int>? events = null)
    {
        if (vehicle.Components.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            return await _vehicleEventRepository.GetAllEventsByVehicleId(privateVehicleComponent.Id, events);
        }
        return new();
    }

    public async Task<List<VehicleEventData>> GetLastVehicleEvents(RealmVehicle vehicle, int limit = 10, IEnumerable<int>? events = null)
    {
        if (vehicle.Components.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            return await _vehicleEventRepository.GetLastEventsByVehicleId(privateVehicleComponent.Id, limit, events);
        }
        return new();
    }
    
    public IEnumerable<RealmPlayer> GetOnlineOwner(RealmVehicle vehicle)
    {
        if (vehicle.Components.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            var owners = privateVehicleComponent.Access.Owners;
            foreach (var owner in owners)
            {
                if (_activeUsers.TryGetPlayerByUserId(owner.userId, out var player) && player != null)
                    yield return player;
            }
        }
    }
}

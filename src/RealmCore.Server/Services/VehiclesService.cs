using RealmCore.Persistence.DTOs;
using RealmCore.Server.Json.Converters;

namespace RealmCore.Server.Services;

internal sealed class VehiclesService : IVehiclesService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IEntityFactory _entityFactory;
    private readonly ISaveService _saveService;
    private readonly ItemsRegistry _itemsRegistry;
    private readonly IVehicleEventRepository _vehicleEventRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEntityEngine _entityEngine;
    private readonly VehicleUpgradeRegistry _vehicleUpgradeRegistry;
    private readonly VehicleEnginesRegistry _vehicleEnginesRegistry;
    private readonly IActiveUsers _activeUsers;
    private readonly IActiveVehicles _activeVehicles;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public VehiclesService(IVehicleRepository vehicleRepository, IEntityFactory entityFactory, ISaveService saveService, ItemsRegistry itemsRegistry, IVehicleEventRepository vehicleEventRepository, IDateTimeProvider dateTimeProvider, IEntityEngine entityEngine, VehicleUpgradeRegistry vehicleUpgradeRegistry, VehicleEnginesRegistry vehicleEnginesRegistry, IActiveUsers activeUsers, IActiveVehicles activeVehicles)
    {
        _vehicleRepository = vehicleRepository;
        _entityFactory = entityFactory;
        _saveService = saveService;
        _itemsRegistry = itemsRegistry;
        _vehicleEventRepository = vehicleEventRepository;
        _dateTimeProvider = dateTimeProvider;
        _entityEngine = entityEngine;
        _vehicleUpgradeRegistry = vehicleUpgradeRegistry;
        _vehicleEnginesRegistry = vehicleEnginesRegistry;
        _activeUsers = activeUsers;
        _activeVehicles = activeVehicles;
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { DoubleConverter.Instance }
        };
    }

    public async Task<Entity> CreateVehicle(ushort model, Vector3 position, Vector3 rotation)
    {
        var vehicleData = await _vehicleRepository.CreateVehicle(model, _dateTimeProvider.Now);
        return _entityFactory.CreateVehicle(model, position, rotation, entityBuilder: entity =>
        {
            entity.AddComponent(new PrivateVehicleComponent(vehicleData));
        });
    }

    public async Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity)
    {
        if (!vehicleEntity.HasComponent<VehicleTagComponent>())
            throw new InvalidOperationException();

        if (vehicleEntity.HasComponent<PrivateVehicleComponent>())
            throw new InvalidOperationException();

        var vehicleElementComponent = vehicleEntity.GetRequiredComponent<VehicleElementComponent>();
        vehicleEntity.AddComponent(new PrivateVehicleComponent(await _vehicleRepository.CreateVehicle(vehicleElementComponent.Model, _dateTimeProvider.Now)));
        return vehicleEntity;
    }

    public async Task<List<LightInfoVehicleDTO>> GetAllLightVehicles(Entity entity)
    {
        if (entity.TryGetComponent(out UserComponent userComponent))
        {
            return await _vehicleRepository.GetLightVehiclesByUserId(userComponent.Id).ConfigureAwait(false);
        }
        return new();
    }

    public async Task<List<VehicleData>> GetAllVehicles(Entity entity)
    {
        if(entity.TryGetComponent(out UserComponent userComponent))
        {
            return await _vehicleRepository.GetVehiclesByUserId(userComponent.Id).ConfigureAwait(false);
        }
        return new();
    }

    public Task<List<VehicleData>> GetAllSpawnedVehicles()
    {
        return _vehicleRepository.GetAllSpawnedVehicles();
    }

    public async Task Destroy(Entity entity)
    {
        if (!entity.HasComponent<VehicleTagComponent>())
            throw new InvalidOperationException("Entity is not vehicle");
        await _vehicleRepository.SetSpawned(entity.GetRequiredComponent<PrivateVehicleComponent>().Id, false);
        await _saveService.BeginSave(entity);
        await _saveService.Commit();
        entity.Dispose();
    }

    public Task<bool> SetVehicleKind(Entity vehicleEntity, byte kind)
        => SetVehicleKind(vehicleEntity.GetRequiredComponent<PrivateVehicleComponent>().Id, kind);

    public Task<bool> SetVehicleKind(int id, byte kind)
    {
        if(_entityEngine.GetVehicleById(id, out var entity))
            entity.GetRequiredComponent<PrivateVehicleComponent>().Kind = kind;

        return _vehicleRepository.SetKind(id, kind);
    }
    
    public async Task<bool> SetVehicleSpawned(Entity vehicleEntity, bool spawned = true)
    {
        if (vehicleEntity.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            return await _vehicleRepository.SetSpawned(privateVehicleComponent.Id, spawned).ConfigureAwait(false);
        }
        return false;
    }

    public async Task<VehicleAccess?> GetVehicleAccess(Entity vehicleEntity)
    {
        if (vehicleEntity.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            var vehiclesAccesses = await _vehicleRepository.GetAllVehicleAccesses(privateVehicleComponent.Id);
            return new VehicleAccess(vehiclesAccesses);
        }
        return null;
    }

    public async Task<Entity> Spawn(VehicleData vehicleData)
    {
        if(vehicleData.IsRemoved)
            throw new VehicleRemovedException(vehicleData.Id);

        var vehicleEntity = _entityFactory.CreateVehicle(vehicleData.Model, vehicleData.TransformAndMotion.Position, vehicleData.TransformAndMotion.Rotation, vehicleData.TransformAndMotion.Interior, vehicleData.TransformAndMotion.Dimension,
            entity =>
            {
                if (!_activeVehicles.TrySetActive(vehicleData.Id, entity))
                    throw new Exception("Failed to create already existing vehicle.");

                entity.AddComponent(new PrivateVehicleComponent(vehicleData));
                entity.AddComponent(new VehicleUpgradesComponent(vehicleData.Upgrades));
                entity.AddComponent(new MileageCounterComponent(vehicleData.Mileage));
                if (vehicleData.VehicleEngines.Count != 0)
                    entity.AddComponent(new VehicleEngineComponent(vehicleData.VehicleEngines));
                else
                    entity.AddComponent<VehicleEngineComponent>();
                entity.AddComponent(new VehiclePartDamageComponent(vehicleData.PartDamages));

                if (vehicleData.Fuels.Count != 0)
                {
                    foreach (var vehicleFuel in vehicleData.Fuels)
                        entity.AddComponent(new FuelComponent(vehicleFuel.FuelType, vehicleFuel.Amount, vehicleFuel.MaxCapacity, vehicleFuel.FuelConsumptionPerOneKm, vehicleFuel.MinimumDistanceThreshold)).Active = vehicleFuel.Active;
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
                        entity.AddComponent(new InventoryComponent(inventory.Size, inventory.Id, items));
                    }
                }

            });

        await SetVehicleSpawned(vehicleEntity);
        return vehicleEntity;
    }

    public async Task<bool> AddVehicleEvent(Entity vehicleEntity, int eventId, string? metadata = null)
    {
        if(vehicleEntity.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            await _vehicleEventRepository.AddEvent(privateVehicleComponent.Id, eventId, _dateTimeProvider.Now).ConfigureAwait(false);
            return true;
        }
        return false;
    }

    public async Task<List<VehicleEventData>> GetAllVehicleEvents(Entity vehicleEntity, IEnumerable<int>? events = null)
    {
        if (vehicleEntity.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            return await _vehicleEventRepository.GetAllEventsByVehicleId(privateVehicleComponent.Id, events).ConfigureAwait(false);
        }
        return new();
    }

    public async Task<List<VehicleEventData>> GetLastVehicleEvents(Entity vehicleEntity, int limit = 10, IEnumerable<int>? events = null)
    {
        if (vehicleEntity.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            return await _vehicleEventRepository.GetLastEventsByVehicleId(privateVehicleComponent.Id, limit, events).ConfigureAwait(false);
        }
        return new();
    }
    
    public IEnumerable<Entity> GetOnlineOwner(Entity vehicleEntity)
    {
        if (vehicleEntity.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
        {
            var owners = privateVehicleComponent.Access.Owners;
            foreach (var owner in owners)
            {
                if (_activeUsers.TryGetEntityByUserId(owner.userId, out var playerEntity) && playerEntity != null)
                    yield return playerEntity;
            }
        }
    }
}

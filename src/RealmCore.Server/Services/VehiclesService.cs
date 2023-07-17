using RealmCore.Persistance.Data;
using RealmCore.Persistance.DTOs;
using RealmCore.Persistance.Interfaces;
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
    private readonly IECS _ecs;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public VehiclesService(IVehicleRepository vehicleRepository, IEntityFactory entityFactory, ISaveService saveService, ItemsRegistry itemsRegistry, IVehicleEventRepository vehicleEventRepository, IDateTimeProvider dateTimeProvider, IECS ecs)
    {
        _vehicleRepository = vehicleRepository;
        _entityFactory = entityFactory;
        _saveService = saveService;
        _itemsRegistry = itemsRegistry;
        _vehicleEventRepository = vehicleEventRepository;
        _dateTimeProvider = dateTimeProvider;
        _ecs = ecs;
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new DoubleConverter() }
        };
    }

    public async Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity)
    {
        if (vehicleEntity.Tag != EntityTag.Vehicle)
            throw new InvalidOperationException();

        if (vehicleEntity.HasComponent<PrivateVehicleComponent>())
            throw new InvalidOperationException();

        var vehicleElementComponent = vehicleEntity.GetRequiredComponent<VehicleElementComponent>();
        vehicleEntity.AddComponent(new PrivateVehicleComponent(await _vehicleRepository.CreateNewVehicle(vehicleElementComponent.Model, _dateTimeProvider.Now)));
        return vehicleEntity;
    }

    public Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId)
    {
        return _vehicleRepository.GetLightVehiclesByUserId(userId);
    }

    public Task<LightInfoVehicleDTO?> GetLightVehicleById(int vehicleId)
    {
        return _vehicleRepository.GetLightVehicleById(vehicleId);
    }

    public Task<List<VehicleData>> GetVehiclesByUserId(int userId)
    {
        return _vehicleRepository.GetVehiclesByUserId(userId);
    }

    public Task<List<VehicleData>> GetAllSpawnedVehicles()
    {
        return _vehicleRepository.GetAllSpawnedVehicles();
    }

    public Task<VehicleData?> GetVehicleById(int id)
    {
        return _vehicleRepository.GetReadOnlyVehicleById(id);
    }

    public async Task Destroy(Entity entity)
    {
        if (entity.Tag != EntityTag.Vehicle)
            throw new InvalidOperationException("Entity is not vehicle");
        await _vehicleRepository.SetSpawned(entity.GetRequiredComponent<PrivateVehicleComponent>().Id, false);
        await _saveService.Save(entity);
        await _saveService.Commit();
        entity.Dispose();
    }

    public Task<bool> SetVehicleKind(Entity vehicleEntity, byte kind)
        => SetVehicleKind(vehicleEntity.GetRequiredComponent<PrivateVehicleComponent>().Id, kind);

    public Task<bool> SetVehicleKind(int id, byte kind)
    {
        if(_ecs.GetVehicleById(id, out var entity))
            entity.GetRequiredComponent<PrivateVehicleComponent>().Kind = kind;

        return _vehicleRepository.SetKind(id, kind);
    }
    
    public Task<bool> SetVehicleSpawned(Entity vehicleEntity, bool spawned = true)
        => SetVehicleSpawned(vehicleEntity.GetRequiredComponent<PrivateVehicleComponent>().Id, spawned);

    public Task<bool> SetVehicleSpawned(int id, bool spawned = true)
    {
        return _vehicleRepository.SetSpawned(id, spawned);
    }

    public async Task<VehicleAccess> GetVehicleAccess(int vehicleId)
    {
        var vehiclesAccesses = await _vehicleEventRepository.GetAllVehicleAccesses(vehicleId);
        return new VehicleAccess(vehiclesAccesses);
    }

    public Entity Spawn(VehicleData vehicleData)
    {
        var entity = _entityFactory.CreateVehicle(vehicleData.Model, vehicleData.TransformAndMotion.Position, vehicleData.TransformAndMotion.Rotation, new ConstructionInfo
        {
            Id = $"vehicle {vehicleData.Id}",
            Interior = vehicleData.TransformAndMotion.Interior,
            Dimension = vehicleData.TransformAndMotion.Dimension,
        },
            entity =>
            {
                entity.AddComponent(new PrivateVehicleComponent(vehicleData));
                entity.AddComponent(new VehicleUpgradesComponent(vehicleData.Upgrades));
                entity.AddComponent(new MileageCounterComponent(vehicleData.Mileage));
                if (vehicleData.VehicleEngines.Any())
                    entity.AddComponent(new VehicleEngineComponent(vehicleData.VehicleEngines));
                else
                    entity.AddComponent<VehicleEngineComponent>();
                entity.AddComponent(new VehiclePartDamageComponent(vehicleData.PartDamages));

                if (vehicleData.Fuels.Any())
                {
                    foreach (var vehicleFuel in vehicleData.Fuels)
                        entity.AddComponent(new VehicleFuelComponent(vehicleFuel.FuelType, vehicleFuel.Amount, vehicleFuel.MaxCapacity, vehicleFuel.FuelConsumptionPerOneKm, vehicleFuel.MinimumDistanceThreshold)).Active = vehicleFuel.Active;
                }

                if (vehicleData.Inventories != null && vehicleData.Inventories.Any())
                {
                    foreach (var inventory in vehicleData.Inventories)
                    {
                        var items = inventory.InventoryItems
                            .Select(x =>
                                new Item(_itemsRegistry, x.ItemId, x.Number, JsonConvert.DeserializeObject<Dictionary<string, object>>(x.MetaData, _jsonSerializerSettings))
                            )
                            .ToList();
                        entity.AddComponent(new InventoryComponent(inventory.Size, inventory.Id, items));
                    }
                }

            });

        return entity;
    }

    public async Task AddVehicleEvent(int id, int eventId)
    {
        _vehicleEventRepository.AddEvent(id, eventId, _dateTimeProvider.Now);
        await _vehicleEventRepository.Commit();
    }

    public async Task AddVehicleEvent(Entity entity, int eventId)
    {
        await AddVehicleEvent(entity.GetRequiredComponent<PrivateVehicleComponent>().Id, eventId);
    }

    public Task<List<VehicleEventDTO>> GetAllVehicleEvents(int id) => _vehicleEventRepository.GetAllEventsByVehicleId(id);

    public Task<List<VehicleEventDTO>> GetAllVehicleEvents(Entity entity) => _vehicleEventRepository.GetAllEventsByVehicleId(entity.GetRequiredComponent<PrivateVehicleComponent>().Id);
}

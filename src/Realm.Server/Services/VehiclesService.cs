using Realm.Persistance.DTOs;
using Realm.Persistance.Interfaces;

namespace Realm.Server.Services;

internal sealed class VehiclesService : IVehiclesService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IEntityFactory _entityFactory;
    private readonly ISaveService _saveService;
    private readonly ItemsRegistry _itemsRegistry;
    private readonly IDateTimeProvider _dateTimeProvider;

    public VehiclesService(IVehicleRepository vehicleRepository, IEntityFactory entityFactory, ISaveService saveService, ItemsRegistry itemsRegistry, IDateTimeProvider dateTimeProvider)
    {
        _vehicleRepository = vehicleRepository;
        _entityFactory = entityFactory;
        _saveService = saveService;
        _itemsRegistry = itemsRegistry;
        _dateTimeProvider = dateTimeProvider;
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

    public async Task Despawn(Entity entity)
    {
        await _vehicleRepository.SetSpawned(entity.GetRequiredComponent<PrivateVehicleComponent>().Id, false);
        await _saveService.Save(entity);
        entity.Dispose();
    }

    public async Task<Entity?> SpawnById(int id)
    {
        var vehicle = await _vehicleRepository.GetReadOnlyVehicleById(id);
        if (vehicle == null)
            return null;
        if(vehicle.Spawned == false)
        {
            await _vehicleRepository.SetSpawned(id, true);
            return Spawn(vehicle);
        }
        return null;
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
                                new Item(_itemsRegistry, x.ItemId, x.Number, JsonConvert.DeserializeObject<Dictionary<string, object>>(x.MetaData))
                            )
                            .ToList();
                        entity.AddComponent(new InventoryComponent(inventory.Size, inventory.Id, items));
                    }
                }

            });

        return entity;
    }
}

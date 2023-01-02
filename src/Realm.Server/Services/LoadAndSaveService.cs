using Microsoft.AspNetCore.Identity;
using Realm.Domain.Components.Vehicles;
using Realm.Persistance.Data;
using static Realm.Persistance.Data.Helpers.VehicleDamageState;
using static Realm.Persistance.Data.Helpers.VehicleWheelStatus;

namespace Realm.Server.Services;

internal class LoadAndSaveService : ILoadAndSaveService
{
    private readonly RealmDbContextFactory _dbContextFactory;
    private readonly RepositoryFactory _repositoryFactory;
    private readonly IEntityFactory _entityFactory;
    private readonly UserManager<User> _userManager;
    private readonly ECS _ecs;

    public LoadAndSaveService(RealmDbContextFactory dbContextFactory, RepositoryFactory repositoryFactory, IEntityFactory entityFactory, UserManager<User> userManager, ECS ecs)
    {
        _dbContextFactory = dbContextFactory;
        _repositoryFactory = repositoryFactory;
        _entityFactory = entityFactory;
        _userManager = userManager;
        _ecs = ecs;
        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        entity.Destroyed += HandleEntityDestroyed;
    }

    private async void HandleEntityDestroyed(Entity entity)
    {
        await Save(entity);
    }

    public async ValueTask<bool> Save(Entity entity, IDb? context = null)
    {
        if (context == null)
            context = _dbContextFactory.CreateDbContext();

        switch (entity.Tag)
        {
            case Entity.PlayerTag:
                if (entity.TryGetComponent(out AccountComponent accountComponent))
                {
                    var user = accountComponent.User;

                    if (entity.TryGetComponent(out MoneyComponent moneyComponent))
                        user.Money = moneyComponent.Money;

                    if (entity.TryGetComponent(out LicensesComponent licensesComponent))
                        user.Licenses = licensesComponent.Licenses;

                    if (entity.TryGetComponent(out PlayTimeComponent playTimeComponent))
                    {
                        user.PlayTime += playTimeComponent.PlayTime;
                        playTimeComponent.Reset();
                    }

                    if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
                    {
                        bool updateInventory = true;
                        if (user.Inventory == null)
                        {
                            user.Inventory = new Inventory
                            {
                                Size = inventoryComponent.Size,
                                Id = inventoryComponent.Id,
                                InventoryItems = new List<InventoryItem>()
                            };
                            updateInventory = false;
                            //context.Inventories.Add(user.Inventory);
                        }

                        user.Inventory.Size = inventoryComponent.Size;
                        user.Inventory.InventoryItems = inventoryComponent.Items.Select(item => new InventoryItem
                        {
                            Id = item.Id ?? Guid.NewGuid().ToString(),
                            Inventory = user.Inventory,
                            InventoryId = user.Inventory.Id,
                            ItemId = item.ItemId,
                            Number = item.Number,
                            MetaData = JsonConvert.SerializeObject(item.MetaData, Formatting.None),
                        }).ToList();
                        if (updateInventory)
                        {
                            user.Inventory.Id = inventoryComponent.Id;
                            //context.Inventories.Update(user.Inventory);
                        }
                    }
                    user.LastTransformAndMotion = entity.Transform.GetTransformAndMotion();
                    await _userManager.UpdateAsync(user);
                    return true;
                }
                break;
            case Entity.VehicleTag:
                if (entity.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
                {
                    var vehicleElementComponent = entity.GetRequiredComponent<VehicleElementComponent>();
                    var vehicle = vehicleElementComponent.Vehicle;
                    var vehicleData = privateVehicleComponent.VehicleData;

                    vehicleData.Model = vehicle.Model;
                    vehicleData.Color = new Persistance.Data.Helpers.VehicleColor(vehicle.Colors.Primary, vehicle.Colors.Secondary, vehicle.Colors.Color3, vehicle.Colors.Color4, vehicle.HeadlightColor);
                    vehicleData.Paintjob = vehicle.PaintJob;
                    vehicleData.Platetext = vehicle.PlateText;
                    vehicleData.Variant = new Persistance.Data.Helpers.VehicleVariant(vehicle.Variants.Variant1, vehicle.Variants.Variant2);
                    vehicleData.DamageState = new Persistance.Data.Helpers.VehicleDamageState
                    {
                        FrontLeftLight = (LightState)vehicle.Damage.Lights[0],
                        FrontRightLight = (LightState)vehicle.Damage.Lights[1],
                        RearLeftLight = (LightState)vehicle.Damage.Lights[2],
                        RearRightLight = (LightState)vehicle.Damage.Lights[3],
                        FrontLeftPanel = (PanelState)vehicle.Damage.Panels[0],
                        FrontRightPanel = (PanelState)vehicle.Damage.Panels[1],
                        RearLeftPanel = (PanelState)vehicle.Damage.Panels[2],
                        RearRightPanel = (PanelState)vehicle.Damage.Panels[3],
                        Windscreen = (PanelState)vehicle.Damage.Panels[4],
                        FrontBumper = (PanelState)vehicle.Damage.Panels[5],
                        RearBumper = (PanelState)vehicle.Damage.Panels[6],
                        Hood = (DoorState)vehicle.Damage.Doors[0],
                        Trunk = (DoorState)vehicle.Damage.Doors[1],
                        FrontLeftDoor = (DoorState)vehicle.Damage.Doors[2],
                        FrontRightDoor = (DoorState)vehicle.Damage.Doors[3],
                        RearLeftDoor = (DoorState)vehicle.Damage.Doors[4],
                        RearRightDoor = (DoorState)vehicle.Damage.Doors[5],
                    };
                    vehicleData.DoorOpenRatio = new Persistance.Data.Helpers.VehicleDoorOpenRatio
                    {
                        Hood = vehicle.DoorRatios[0],
                        Trunk = vehicle.DoorRatios[1],
                        FrontLeft = vehicle.DoorRatios[2],
                        FrontRight = vehicle.DoorRatios[3],
                        RearLeft = vehicle.DoorRatios[4],
                        RearRight = vehicle.DoorRatios[5],
                    };
                    vehicleData.WheelStatus = new Persistance.Data.Helpers.VehicleWheelStatus
                    {
                        FrontLeft = (WheelStatus)vehicle.GetWheelState(SlipeServer.Packets.Enums.VehicleWheel.FrontLeft),
                        FrontRight = (WheelStatus)vehicle.GetWheelState(SlipeServer.Packets.Enums.VehicleWheel.FrontRight),
                        RearLeft = (WheelStatus)vehicle.GetWheelState(SlipeServer.Packets.Enums.VehicleWheel.RearLeft),
                        RearRight = (WheelStatus)vehicle.GetWheelState(SlipeServer.Packets.Enums.VehicleWheel.RearLeft),
                    };
                    vehicleData.EngineState = vehicle.IsEngineOn;
                    vehicleData.LandingGearDown = vehicle.IsLandingGearDown;
                    vehicleData.OverrideLights = (byte)vehicle.OverrideLights;
                    vehicleData.SirensState = vehicle.IsSirenActive;
                    vehicleData.Locked = vehicle.IsLocked;
                    vehicleData.TaxiLightState = vehicle.IsTaxiLightOn;
                    vehicleData.Health = vehicle.Health;
                    vehicleData.IsFrozen = vehicle.IsFrozen;
                    vehicleData.TransformAndMotion = entity.Transform.GetTransformAndMotion();
                    context.Vehicles.Update(vehicleData);
                    System.Diagnostics.Debug.Assert(await context.SaveChangesAsync() == 1);
                }
                break;
        }
        return false;
    }

    public async Task<int> SaveAll()
    {
        int savedEntities = 0;
        using var context = _dbContextFactory.CreateDbContext();
        foreach (var entity in _ecs.Entities)
        {
            await Save(entity, context);
            savedEntities++;
        }
        return savedEntities;
    }

    public async Task LoadAll()
    {
        // Load vehicles
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            foreach (var vehicleData in await vehicleRepository.GetAll().AsNoTrackingWithIdentityResolution().ToListAsync())
            {
                var entity = _entityFactory.CreateVehicle(vehicleData.Model, vehicleData.TransformAndMotion.Position, vehicleData.TransformAndMotion.Rotation, vehicleData.TransformAndMotion.Interior, vehicleData.TransformAndMotion.Dimension, $"vehicle {vehicleData.Id}");
                entity.AddComponent(new PrivateVehicleComponent(vehicleData));
            }
        }
    }

}

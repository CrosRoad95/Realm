using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Realm.Domain.Components.Vehicles;
using Realm.Persistance.Data;
using System.Runtime.ConstrainedExecution;
using static Realm.Persistance.Data.Helpers.VehicleDamageState;
using static Realm.Persistance.Data.Helpers.VehicleWheelStatus;

namespace Realm.Server.Services;

internal class LoadAndSaveService : ILoadAndSaveService
{
    private readonly RealmDbContextFactory _dbContextFactory;
    private readonly RepositoryFactory _repositoryFactory;
    private readonly IEntityFactory _entityFactory;
    private readonly ILogger _logger;

    public LoadAndSaveService(RealmDbContextFactory dbContextFactory, RepositoryFactory repositoryFactory, IEntityFactory entityFactory,
        ILogger logger)
    {
        _dbContextFactory = dbContextFactory;
        _repositoryFactory = repositoryFactory;
        _entityFactory = entityFactory;
        _logger = logger;
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
                    var user = await context.Users
                        .Include(x => x.Licenses)
                        .Include(x => x.JobUpgrades)
                        .Include(x => x.Achievements)
                        .Include(x => x.DailyVisits)
                        .Include(x => x.Statistics)
                        .Include(x => x.Inventories)
                        .ThenInclude(x => x!.InventoryItems)
                        .Where(x => x.Id == accountComponent.Id).FirstAsync();
                    user.LastTransformAndMotion = entity.Transform.GetTransformAndMotion();

                    if (entity.TryGetComponent(out MoneyComponent moneyComponent))
                        user.Money = moneyComponent.Money;
                    else
                        user.Money = 0;

                    if (entity.TryGetComponent(out LicensesComponent licensesComponent))
                    {
                        user.Licenses = licensesComponent.Licenses.Select(x => new UserLicense
                        {
                            User = user,
                            LicenseId = x.licenseId,
                            SuspendedReason = x.suspendedReason,
                            SuspendedUntil = x.suspendedUntil,
                        }).ToList();
                    }
                    else
                    {
                        user.Licenses = new List<UserLicense>();
                    }

                    if (entity.TryGetComponent(out PlayTimeComponent playTimeComponent))
                    {
                        user.PlayTime = playTimeComponent.TotalPlayTime;
                        playTimeComponent.Reset();
                    }
                    else
                        user.PlayTime = 0;

                    var inventoryComponents = entity.Components.OfType<InventoryComponent>().ToList();
                    user.Inventories = inventoryComponents.Select(x =>
                    {
                        var inventory = new Inventory
                        {
                            Size = x.Size,
                            Id = x.Id ?? Guid.Empty,
                        };
                        inventory.InventoryItems = x.Items.Select(item => new InventoryItem
                        {
                            Id = item.Id ?? Guid.NewGuid().ToString(),
                            ItemId = item.ItemId,
                            Number = item.Number,
                            Inventory = inventory,
                            MetaData = JsonConvert.SerializeObject(item.MetaData, Formatting.None),
                        }).ToList();
                        return inventory;
                    }).ToList();


                    if (entity.TryGetComponent(out DailyVisitsCounterComponent dailyVisitsCounterComponent))
                    {
                        user.DailyVisits = new DailyVisits
                        {
                            UserId = user.DailyVisits?.UserId ?? Guid.Empty,
                            LastVisit = dailyVisitsCounterComponent.LastVisit,
                            VisitsInRow = dailyVisitsCounterComponent.VisitsInRow,
                            VisitsInRowRecord = dailyVisitsCounterComponent.VisitsInRowRecord,
                        };
                    }
                    else
                        user.DailyVisits = null;

                    if (entity.TryGetComponent(out StatisticsCounterComponent statisticsCounterComponent))
                    {
                        user.Statistics = new Statistics
                        {
                            UserId = user.Statistics?.UserId ?? Guid.Empty,
                            TraveledDistanceByFoot = statisticsCounterComponent.TraveledDistanceByFoot,
                            TraveledDistanceInAir = statisticsCounterComponent.TraveledDistanceInAir,
                            TraveledDistanceInVehicleAsDriver = statisticsCounterComponent.TraveledDistanceInVehicleAsDriver,
                            TraveledDistanceInVehicleAsPassager = statisticsCounterComponent.TraveledDistanceInVehicleAsPassager,
                            TraveledDistanceSwimming = statisticsCounterComponent.TraveledDistanceSwimming,
                        };
                    }
                    else
                        user.Statistics = null;

                    if (entity.TryGetComponent(out AchievementsComponent achievementsComponent))
                    {
                        user.Achievements = achievementsComponent.Achievements.Select(x => new Achievement
                        {
                            Name = x.Key,
                            Value = JsonConvert.SerializeObject(x.Value.Value, Formatting.None),
                            PrizeReceived = x.Value.PrizeReceived,
                            Progress = x.Value.Progress,
                        }).ToList();
                    }
                    else
                        user.Achievements = new List<Achievement>();

                    if (entity.TryGetComponent(out JobUpgradesComponent jobUpgradesComponent))
                    {
                        user.JobUpgrades = jobUpgradesComponent.Upgrades.Select(x => new JobUpgrade
                        {
                            Name = x.name,
                            JobId = x.jobId,
                        }).ToList();
                    }
                    else
                        user.JobUpgrades = new List<JobUpgrade>();

                    await context.SaveChangesAsync();
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
                    vehicleData.VehicleAccesses = privateVehicleComponent.VehicleAccesses.Select(x => new VehicleAccess
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        VehicleId = vehicleData.UserId,
                        Vehicle = vehicleData,
                        Description = new Persistance.Data.Helpers.VehicleAccessDescription
                        {
                            Ownership = x.Ownership,
                        },
                    }).ToList();

                    await context.VehicleUpgrades.Where(x => x.VehicleId == vehicleData.UserId).ExecuteDeleteAsync();
                    if (entity.TryGetComponent(out VehicleUpgradesComponent vehicleUpgradesComponent))
                    {
                        vehicleData.Upgrades = vehicleUpgradesComponent.Upgrades.Select(x => new Persistance.Data.VehicleUpgrade
                        {
                            UpgradeId = x,
                            Vehicle = vehicleData,
                            VehicleId = vehicleData.UserId
                        }).ToList();
                        context.VehicleUpgrades.AddRange(vehicleData.Upgrades);
                    }

                    await context.VehicleFuels.Where(x => x.VehicleId == vehicleData.UserId).ExecuteDeleteAsync();
                    var fuelComponents = entity.Components.OfType<VehicleFuelComponent>();
                    vehicleData.Fuels = fuelComponents.Select(x => new VehicleFuel
                    {
                        Vehicle = vehicleData,
                        VehicleId = vehicleData.UserId,
                        FuelType = x.FuelType,
                        Active = x.Active,
                        Amount = x.Amount,
                        FuelConsumptionPerOneKm = x.FuelConsumptionPerOneKm,
                        MaxCapacity = x.MaxCapacity,
                        MinimumDistanceThreshold = x.MinimumDistanceThreshold,
                    }).ToList();
                    context.VehicleFuels.AddRange(vehicleData.Fuels);

                    context.Vehicles.Update(vehicleData);
                    await context.SaveChangesAsync();
                }
                break;
        }
        return false;
    }

    public async Task LoadAll()
    {
        // Load vehicles
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var results = await vehicleRepository
                .GetAll()
                .Include(x => x.Fuels)
                .Include(x => x.Upgrades)
                .Include(x => x.VehicleAccesses)
                .ThenInclude(x => x.User)
                .AsNoTrackingWithIdentityResolution()
                .ToListAsync();

            foreach (var vehicleData in results)
            {
                try
                {
                    await Task.Delay(200);
                    var entity = _entityFactory.CreateVehicle(vehicleData.Model, vehicleData.TransformAndMotion.Position, vehicleData.TransformAndMotion.Rotation, vehicleData.TransformAndMotion.Interior, vehicleData.TransformAndMotion.Dimension, $"vehicle {vehicleData.UserId}",
                        entity =>
                        {
                            entity.AddComponent(new PrivateVehicleComponent(vehicleData));
                            entity.AddComponent(new VehicleUpgradesComponent(vehicleData.Upgrades));
                            foreach (var vehicleFuel in vehicleData.Fuels)
                                entity.AddComponent(new VehicleFuelComponent(vehicleFuel.FuelType, vehicleFuel.Amount, vehicleFuel.MaxCapacity, vehicleFuel.FuelConsumptionPerOneKm, vehicleFuel.MinimumDistanceThreshold)).Active = vehicleFuel.Active;
                        });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            _logger.Information("Loaded: {amount} vehicles", results.Count);

        }
    }
}

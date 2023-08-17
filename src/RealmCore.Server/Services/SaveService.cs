using RealmCore.Persistence.Data;
using RealmCore.Persistence.Data.Helpers;
using RealmCore.Persistence.Extensions;
using static RealmCore.Persistence.Data.Helpers.VehicleDamageState;
using static RealmCore.Persistence.Data.Helpers.VehicleWheelStatus;

namespace RealmCore.Server.Services;

internal class SaveService : ISaveService
{
    private readonly IDb _dbContext;
    private readonly IEnumerable<IUserDataSaver> _userDataSavers;

    public SaveService(IDb dbContext, IEnumerable<IUserDataSaver> userDataSavers)
    {
        _dbContext = dbContext;
        _userDataSavers = userDataSavers;
    }

    private async Task SaveVehicle(Entity entity)
    {
        if (!entity.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
            return;

        var vehicleData = await _dbContext.Vehicles
            .IncludeAll()
            .Where(x => x.Id == privateVehicleComponent.Id)
            .FirstAsync();

        var vehicleElementComponent = entity.GetRequiredComponent<VehicleElementComponent>();
        var vehicle = vehicleElementComponent.Vehicle;

        vehicleData.Model = vehicle.Model;
        vehicleData.Color = new VehicleColor(vehicle.Colors.Primary, vehicle.Colors.Secondary, vehicle.Colors.Color3, vehicle.Colors.Color4, vehicle.HeadlightColor);
        vehicleData.Paintjob = vehicle.PaintJob;
        vehicleData.Platetext = vehicle.PlateText;
        vehicleData.Variant = new VehicleVariant(vehicle.Variants.Variant1, vehicle.Variants.Variant2);
        vehicleData.DamageState = new VehicleDamageState
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
        vehicleData.DoorOpenRatio = new VehicleDoorOpenRatio
        {
            Hood = vehicle.DoorRatios[0],
            Trunk = vehicle.DoorRatios[1],
            FrontLeft = vehicle.DoorRatios[2],
            FrontRight = vehicle.DoorRatios[3],
            RearLeft = vehicle.DoorRatios[4],
            RearRight = vehicle.DoorRatios[5],
        };
        vehicleData.WheelStatus = new VehicleWheelStatus
        {
            FrontLeft = (WheelStatus)vehicle.GetWheelState(VehicleWheel.FrontLeft),
            FrontRight = (WheelStatus)vehicle.GetWheelState(VehicleWheel.FrontRight),
            RearLeft = (WheelStatus)vehicle.GetWheelState(VehicleWheel.RearLeft),
            RearRight = (WheelStatus)vehicle.GetWheelState(VehicleWheel.RearLeft),
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
        vehicleData.UserAccesses = privateVehicleComponent.Access.PlayerAccesses.Select(x => new VehicleUserAccessData
        {
            Id = x.Id,
            UserId = x.UserId,
            VehicleId = vehicleData.Id,
            Vehicle = vehicleData,
            AccessType = x.AccessType,
            CustomValue = x.CustomValue
        }).ToList();

        if (entity.TryGetComponent(out VehicleUpgradesComponent vehicleUpgradesComponent))
        {
            vehicleData.Upgrades = vehicleUpgradesComponent.Upgrades.Select(x => new VehicleUpgradeData
            {
                UpgradeId = x,
                VehicleId = vehicleData.Id
            }).ToList();

            vehicleData.Paintjob = vehicleUpgradesComponent.Paintjob;
        }

        {
            var fuelComponents = entity.Components.OfType<FuelComponent>();
            vehicleData.Fuels = fuelComponents.Select(x => new VehicleFuelData
            {
                VehicleId = vehicleData.Id,
                FuelType = x.FuelType,
                Active = x.Active,
                Amount = x.Amount,
                FuelConsumptionPerOneKm = x.FuelConsumptionPerOneKm,
                MaxCapacity = x.MaxCapacity,
                MinimumDistanceThreshold = x.MinimumDistanceThreshold,
            }).ToList();
        }
        if (entity.TryGetComponent(out MileageCounterComponent mileageCounterComponent))
        {
            vehicleData.Mileage = mileageCounterComponent.Mileage;
        }
        if (entity.TryGetComponent(out VehiclePartDamageComponent vehiclePartDamageComponent))
        {
            List<VehiclePartDamageData> vehiclePartDamages = new();
            foreach (var item in vehiclePartDamageComponent.Parts)
            {
                var state = vehiclePartDamageComponent.Get(item);
                if (state != null)
                    vehiclePartDamages.Add(new VehiclePartDamageData
                    {
                        PartId = item,
                        State = state.Value
                    }); 
            }
            vehicleData.PartDamages = vehiclePartDamages;
        }

        if (entity.TryGetComponent(out VehicleEngineComponent vehicleEngineComponent))
        {
            vehicleData.VehicleEngines = vehicleEngineComponent.VehicleEngineIds.Select(x => new VehicleEngineData
            {
                EngineId = (short)x,
                Selected = x == vehicleEngineComponent.ActiveVehicleEngineId
            }).ToList();
        }

        var inventoryComponents = entity.Components.OfType<InventoryComponent>()
        .Where(x => x.Id != 0)
        .ToList();

        if (inventoryComponents.Any())
        {
            foreach (var inventory in vehicleData.Inventories)
            {
                var inventoryComponent = inventoryComponents.Where(x => x.Id == inventory.Id).First();
                inventory.Size = inventoryComponent.Size;
                inventory.InventoryItems = MapItems(inventoryComponent);
            }
        }
        else
            vehicleData.Inventories = new List<InventoryData>();
    }

    private InventoryData MapInventory(InventoryComponent inventoryComponent)
    {
        var inventory = new InventoryData
        {
            Size = inventoryComponent.Size,
            Id = inventoryComponent.Id,
            InventoryItems = MapItems(inventoryComponent),
        };
        return inventory;
    }

    private List<InventoryItemData> MapItems(InventoryComponent inventoryComponent)
    {
        var items = inventoryComponent.Items.Select(item => new InventoryItemData
        {
            Id = item.Id,
            ItemId = item.ItemId,
            Number = item.Number,
            MetaData = JsonConvert.SerializeObject(item.MetaData, Formatting.None),
        }).ToList();

        return items;
    }

    public async Task<int> SaveNewPlayerInventory(InventoryComponent inventoryComponent, int userId)
    {
        var inventory = MapInventory(inventoryComponent);
        _dbContext.UserInventories.Add(new UserInventoryData
        {
            Inventory = inventory,
            UserId = userId
        });
        await _dbContext.SaveChangesAsync();
        return inventory.Id;
    }

    public async Task<int> SaveNewVehicleInventory(InventoryComponent inventoryComponent, int vehicleId)
    {
        var inventory = MapInventory(inventoryComponent);
        _dbContext.VehicleInventories.Add(new VehicleInventoryData
        {
            Inventory = inventory,
            VehicleId = vehicleId
        });
        await _dbContext.SaveChangesAsync();
        return inventory.Id;
    }

    private async Task SavePlayer(Entity entity)
    {
        if (!entity.TryGetComponent(out UserComponent userComponent))
            return;

        var user = await _dbContext.Users
            .IncludeAll()
            .Where(x => x.Id == userComponent.Id).FirstAsync();

        foreach (var item in _userDataSavers)
            await item.SaveAsync(entity);

        user.Upgrades = userComponent.Upgrades.Select(x => new UserUpgradeData
        {
            UpgradeId = x
        }).ToList();

        user.Settings = userComponent.Settings.Select(x => new UserSettingData
        {
            SettingId = x,
            Value = userComponent.GetSetting(x) ?? ""
        }).ToList();

        if (entity.TryGetComponent(out PlayerElementComponent playerElementComponent) && playerElementComponent.Spawned)
            user.LastTransformAndMotion = entity.Transform.GetTransformAndMotion();

        if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            user.Money = moneyComponent.Money;
        else
            user.Money = 0;

        if (entity.TryGetComponent(out LicensesComponent licensesComponent))
        {
            user.Licenses = licensesComponent.Licenses.Select(x => new UserLicenseData
            {
                LicenseId = x.licenseId,
                SuspendedReason = x.suspendedReason,
                SuspendedUntil = x.suspendedUntil,
            }).ToList();
        }
        else
        {
            user.Licenses = new List<UserLicenseData>();
        }

        if (entity.TryGetComponent(out PlayTimeComponent playTimeComponent))
        {
            user.PlayTime = (ulong)playTimeComponent.TotalPlayTime.TotalSeconds;
            playTimeComponent.Reset();
        }
        else
            user.PlayTime = 0;

        var inventoryComponents = entity.Components.OfType<InventoryComponent>()
            .Where(x => x.Id != 0)
            .ToList();

        if (inventoryComponents.Any())
        {
            foreach (var inventory in user.Inventories)
            {
                var inventoryComponent = inventoryComponents.Where(x => x.Id == inventory.Id).First();
                inventory.Size = inventoryComponent.Size;
                inventory.InventoryItems = MapItems(inventoryComponent);
            }
        }
        else
            user.Inventories = new List<InventoryData>();


        if (entity.TryGetComponent(out DailyVisitsCounterComponent dailyVisitsCounterComponent))
        {
            user.DailyVisits = new DailyVisitsData
            {
                LastVisit = dailyVisitsCounterComponent.LastVisit,
                VisitsInRow = dailyVisitsCounterComponent.VisitsInRow,
                VisitsInRowRecord = dailyVisitsCounterComponent.VisitsInRowRecord,
            };
        }
        else
            user.DailyVisits = null;

        if (entity.TryGetComponent(out StatisticsCounterComponent statisticsCounterComponent))
        {
            user.Stats = statisticsCounterComponent.GetStatsIds.Select(x => new UserStatData
            {
                StatId = x,
                Value = statisticsCounterComponent.GetStat(x)
            }).ToList();
            _dbContext.UserStats.AddRange(user.Stats);
        }
        else
            user.Stats = new List<UserStatData>();

        if (entity.TryGetComponent(out AchievementsComponent achievementsComponent))
        {
            user.Achievements = achievementsComponent.Achievements.Select(x => new AchievementData
            {
                AchievementId = x.Key,
                Value = JsonConvert.SerializeObject(x.Value.value, Formatting.None),
                PrizeReceived = x.Value.rewardReceived,
                Progress = x.Value.progress,
            }).ToList();
        }
        else
            user.Achievements = new List<AchievementData>();

        if (entity.TryGetComponent(out JobUpgradesComponent jobUpgradesComponent))
        {
            user.JobUpgrades = jobUpgradesComponent.Upgrades.Select(x => new JobUpgradeData
            {
                JobId = x.jobId,
                UpgradeId = x.upgradeId,
            }).ToList();
        }
        else
            user.JobUpgrades = new List<JobUpgradeData>();

        if (entity.TryGetComponent(out JobStatisticsComponent jobStatisticsComponent))
        {
            var newStatistics = jobStatisticsComponent.JobStatistics.Where(x => x.Value.sessionPoints > 0 || x.Value.sessionTimePlayed > 0);
            foreach (var item in newStatistics)
            {
                var first = user.JobStatistics.FirstOrDefault(x => x.JobId == item.Value.jobId && x.Date == jobStatisticsComponent.Date);
                if (first == null)
                {
                    user.JobStatistics.Add(new JobStatisticsData
                    {
                        Date = jobStatisticsComponent.Date,
                        JobId = item.Key,
                        Points = item.Value.sessionPoints,
                        TimePlayed = item.Value.sessionTimePlayed
                    });
                }
                else
                {
                    first.Points += item.Value.sessionPoints;
                    first.TimePlayed += item.Value.sessionTimePlayed;
                }
            }
            jobStatisticsComponent.Reset();
        }
        else
            user.JobStatistics = new List<JobStatisticsData>();

        if (entity.TryGetComponent(out DiscoveriesComponent discoveriesComponent))
        {
            user.Discoveries = discoveriesComponent.Discoveries.Select(x => new DiscoveryData
            {
                DiscoveryId = x,
            }).ToList();
        }

        if (entity.TryGetComponent(out LevelComponent levelComponent))
        {
            user.Level = levelComponent.Level;
            user.Experience = levelComponent.Experience;
        }
        else
        {
            user.Level = 0;
            user.Experience = 0;
        }

        if (entity.TryGetComponent(out DiscordIntegrationComponent discordIntegrationComponent))
        {
            user.DiscordIntegration = new DiscordIntegrationData
            {
                DiscordUserId = discordIntegrationComponent.DiscordUserId
            };
        }
        else
        {
            user.DiscordIntegration = null;
        }
    }

    public async Task<bool> Save(Entity entity)
    {
        if(entity.TryGetComponent(out TagComponent tagComponent))
        {
            switch(tagComponent)
            {
                case PlayerTagComponent:
                    await SavePlayer(entity);
                    break;
                case VehicleTagComponent:
                    await SaveVehicle(entity);
                    break;
                default:
                    return false;
            }
        }
        return true;
    }

    public async Task Commit()
    {
        await _dbContext.SaveChangesAsync();
    }
}

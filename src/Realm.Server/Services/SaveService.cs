using static Realm.Persistance.Data.Helpers.VehicleDamageState;
using static Realm.Persistance.Data.Helpers.VehicleWheelStatus;
using JobUpgrade = Realm.Persistance.Data.JobUpgrade;
using VehicleAccess = Realm.Persistance.Data.VehicleAccess;

namespace Realm.Server.Services;

internal class SaveService : ISaveService
{
    private readonly IDb _dbContext;

    public SaveService(RealmDbContextFactory realmDbContextFactory)
    {
        _dbContext = realmDbContextFactory.CreateDbContext();
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
            UserId = x.UserId ?? 0,
            VehicleId = vehicleData.Id,
            Vehicle = vehicleData,
            Description = new Persistance.Data.Helpers.VehicleAccessDescription
            {
                Ownership = x.Ownership,
            },
        }).ToList();

        if (entity.TryGetComponent(out VehicleUpgradesComponent vehicleUpgradesComponent))
        {
            vehicleData.Upgrades = vehicleUpgradesComponent.Upgrades.Select(x => new Persistance.Data.VehicleUpgrade
            {
                UpgradeId = x,
                VehicleId = vehicleData.Id
            }).ToList();

            vehicleData.Paintjob = vehicleUpgradesComponent.Paintjob;
        }

        {
            var fuelComponents = entity.Components.OfType<VehicleFuelComponent>();
            vehicleData.Fuels = fuelComponents.Select(x => new VehicleFuel
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
            List<VehiclePartDamage> vehiclePartDamages = new();
            foreach (var item in vehiclePartDamageComponent.Parts)
            {
                var state = vehiclePartDamageComponent.Get(item);
                if(state != null)
                    vehiclePartDamages.Add(new VehiclePartDamage
                    {
                        PartId = item,
                        State = state.Value
                    });
            }
            vehicleData.PartDamages = vehiclePartDamages;
        }

        if(entity.TryGetComponent(out VehicleEngineComponent vehicleEngineComponent))
        {
            vehicleData.VehicleEngines = vehicleEngineComponent.VehicleEngineIds.Select(x => new VehicleEngine
            {
                EngineId = (short)x,
                Selected = x == vehicleEngineComponent.ActiveVehicleEngineId
            }).ToList();
        }
    }

    private async Task SavePlayer(Entity entity)
    {
        if (!entity.TryGetComponent(out AccountComponent accountComponent))
            return;

        var user = await _dbContext.Users
            .IncludeAll()
            .Where(x => x.Id == accountComponent.Id).FirstAsync();

        user.Upgrades = accountComponent.Upgrades.Select(x => new UserUpgrade
        {
            UpgradeId = x
        }).ToList();
        
        user.Settings = accountComponent.Settings.Select(x => new UserSetting
        {
            SettingId = x,
            Value = accountComponent.GetSetting(x) ?? ""
        }).ToList();

        if(entity.TryGetComponent(out PlayerElementComponent playerElementComponent) && playerElementComponent.Spawned)
            user.LastTransformAndMotion = entity.Transform.GetTransformAndMotion();

        if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            user.Money = moneyComponent.Money;
        else
            user.Money = 0;
            
        if (entity.TryGetComponent(out LicensesComponent licensesComponent))
        {
            user.Licenses = licensesComponent.Licenses.Select(x => new UserLicense
            {
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
                Id = x.Id ?? 0,
            };
            inventory.InventoryItems = x.Items.Select(item => new InventoryItem
            {
                Id = Guid.NewGuid().ToString(),
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
                LastVisit = dailyVisitsCounterComponent.LastVisit,
                VisitsInRow = dailyVisitsCounterComponent.VisitsInRow,
                VisitsInRowRecord = dailyVisitsCounterComponent.VisitsInRowRecord,
            };
        }
        else
            user.DailyVisits = null;

        if (entity.TryGetComponent(out StatisticsCounterComponent statisticsCounterComponent))
        {
            user.Stats = statisticsCounterComponent.GetStatsIds.Select(x => new UserStat
            {
                StatId = x,
                Value = statisticsCounterComponent.GetStat(x)
            }).ToList();
            _dbContext.UserStats.AddRange(user.Stats);
            ;
        }
        else
            user.Stats = new List<UserStat>();

        if (entity.TryGetComponent(out AchievementsComponent achievementsComponent))
        {
            user.Achievements = achievementsComponent.Achievements.Select(x => new Persistance.Data.Achievement
            {
                AchievementId = x.Key,
                Value = JsonConvert.SerializeObject(x.Value.value, Formatting.None),
                PrizeReceived = x.Value.prizeReceived,
                Progress = x.Value.progress,
            }).ToList();
        }
        else
            user.Achievements = new List<Persistance.Data.Achievement>();

        if (entity.TryGetComponent(out JobUpgradesComponent jobUpgradesComponent))
        {
            user.JobUpgrades = jobUpgradesComponent.Upgrades.Select(x => new JobUpgrade
            {
                JobId = x.jobId,
                UpgradeId = x.upgradeId,
            }).ToList();
        }
        else
            user.JobUpgrades = new List<Persistance.Data.JobUpgrade>();

        if (entity.TryGetComponent(out JobStatisticsComponent jobStatisticsComponent))
        {
            var newStatistics = jobStatisticsComponent.JobStatistics.Where(x => x.Value.sessionPoints > 0 || x.Value.sessionTimePlayed > 0);
            foreach (var item in newStatistics)
            {
                var first = user.JobStatistics.FirstOrDefault(x => x.JobId == item.Value.jobId && x.Date == jobStatisticsComponent.Date);
                if (first == null)
                {
                    user.JobStatistics.Add(new Persistance.Data.JobStatistics
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
            //jobStatisticsComponent.JobStatistics
            //var first = user.JobStatistics.FirstOrDefault(x => x.Date == jobStatisticsComponent.Date);
            //if (first == null)
            //{
            //    user.JobStatistics.Add(new JobStatistics
            //    {
            //        Date= jobStatisticsComponent.Date,
            //        JobId = jobStatisticsComponent
            //    });
            //}
            //else
            //{

            //}
            //user.JobStatistics = jobStatisticsComponent.JobStatistics.Select(x => new JobStatistics
            //{
            //    JobId = x.Key,
            //    Points = x.Value.points,
            //    TimePlayed = x.Value.timePlayed,
            //}).ToList();
        }
        else
            user.JobStatistics = new List<Persistance.Data.JobStatistics>();

        if (entity.TryGetComponent(out DiscoveriesComponent discoveriesComponent))
        {
            user.Discoveries = discoveriesComponent.Discoveries.Select(x => new Discovery
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

        if(entity.TryGetComponent(out DiscordIntegrationComponent discordIntegrationComponent))
        {
            user.DiscordIntegration = new DiscordIntegration
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
        switch (entity.Tag)
        {
            case Entity.EntityTag.Player:
                await SavePlayer(entity);
                break;
            case Entity.EntityTag.Vehicle:
                await SaveVehicle(entity);
                break;
            default:
                return false;
        }
        return true;
    }

    public async Task Commit()
    {
        await _dbContext.SaveChangesAsync();
    }

}

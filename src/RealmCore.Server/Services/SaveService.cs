using RealmCore.Persistence.Data.Helpers;
using static RealmCore.Persistence.Data.Helpers.VehicleDamageState;
using static RealmCore.Persistence.Data.Helpers.VehicleWheelStatus;

namespace RealmCore.Server.Services;

internal sealed class SaveService : ISaveService
{
    private readonly IDb _dbContext;
    private readonly IEnumerable<IUserDataSaver> _userDataSavers;

    public event Action<Element>? ElementSaved;

    public SaveService(IDb dbContext, IEnumerable<IUserDataSaver> userDataSavers)
    {
        _dbContext = dbContext;
        _userDataSavers = userDataSavers;
    }

    private async Task<bool> SaveVehicle(RealmVehicle vehicle)
    {
        if (!vehicle.Components.TryGetComponent(out PrivateVehicleComponent privateVehicleComponent))
            return false;

        var vehicleData = await _dbContext.Vehicles
            .IncludeAll()
            .Where(x => x.Id == privateVehicleComponent.Id)
            .FirstAsync();

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
        vehicleData.TransformAndMotion = vehicle.GetTransformAndMotion();
        vehicleData.UserAccesses = privateVehicleComponent.Access.PlayerAccesses.Select(x => new VehicleUserAccessData
        {
            Id = x.id,
            UserId = x.userId,
            VehicleId = vehicleData.Id,
            Vehicle = vehicleData,
            AccessType = x.accessType,
            CustomValue = x.customValue
        }).ToList();

        if (vehicle.Components.TryGetComponent(out VehicleUpgradesComponent vehicleUpgradesComponent))
        {
            vehicleData.Upgrades = vehicleUpgradesComponent.Upgrades.Select(x => new VehicleUpgradeData
            {
                UpgradeId = x,
                VehicleId = vehicleData.Id
            }).ToList();

        }
        vehicleData.Paintjob = vehicle.PaintJob;

        {
            var fuelComponents = vehicle.Components.ComponentsList.OfType<FuelComponent>();
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
        if (vehicle.Components.TryGetComponent(out MileageCounterComponent mileageCounterComponent))
        {
            vehicleData.Mileage = mileageCounterComponent.Mileage;
        }
        if (vehicle.Components.TryGetComponent(out VehiclePartDamageComponent vehiclePartDamageComponent))
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

        if (vehicle.Components.TryGetComponent(out VehicleEngineComponent vehicleEngineComponent))
        {
            vehicleData.VehicleEngines = vehicleEngineComponent.VehicleEngineIds.Select(x => new VehicleEngineData
            {
                EngineId = (short)x,
                Selected = x == vehicleEngineComponent.ActiveVehicleEngineId
            }).ToList();
        }

        var inventoryComponents = vehicle.Components.ComponentsList
            .OfType<InventoryComponent>()
            .Where(x => x.Id != 0)
            .ToList();

        if (inventoryComponents.Count != 0)
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

        vehicleData.LastUsed = privateVehicleComponent.LastUsed;

        return true;
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

    private async Task<bool> SavePlayer(RealmPlayer player)
    {
        if (!player.IsSignedIn)
            return false;

        var user = player.User;
        var db = player.GetRequiredService<IDb>();
        ;
        var userData = user.User;
        userData.LastTransformAndMotion = player.GetTransformAndMotion();
        db.Users.Update(userData);

        foreach (var item in _userDataSavers)
            await item.SaveAsync(player);

        await db.SaveChangesAsync();
        //var userData = await _dbContext.Users
        //    .IncludeAll()
        //    .Where(x => x.Id == user.Id).FirstAsync();

        //userData.Upgrades = user.Upgrades.Select(x => new UserUpgradeData
        //{
        //    UpgradeId = x
        //}).ToList();

        //userData.Settings = user.Settings.Select(x => new UserSettingData
        //{
        //    SettingId = x,
        //    Value = user.GetSetting(x) ?? ""
        //}).ToList();

        //userData.Money = player.Money.Amount;

        //if (player.TryGetComponent(out LicensesComponent licensesComponent))
        //{
        //    userData.Licenses = licensesComponent.Licenses.Select(x => new UserLicenseData
        //    {
        //        LicenseId = x.licenseId,
        //        SuspendedReason = x.suspendedReason,
        //        SuspendedUntil = x.suspendedUntil,
        //    }).ToList();
        //}
        //else
        //{
        //    userData.Licenses = new List<UserLicenseData>();
        //}

        //if (player.TryGetComponent(out PlayTimeComponent playTimeComponent))
        //{
        //    userData.PlayTime = (ulong)playTimeComponent.TotalPlayTime.TotalSeconds;
        //    playTimeComponent.Reset();
        //}
        //else
        //    userData.PlayTime = 0;

        //var inventoryComponents = player.Components.ComponentsList.OfType<InventoryComponent>()
        //    .Where(x => x.Id != 0)
        //    .ToList();

        //if (inventoryComponents.Count != 0)
        //{
        //    foreach (var inventory in userData.Inventories)
        //    {
        //        var inventoryComponent = inventoryComponents.Where(x => x.Id == inventory.Id).First();
        //        inventory.Size = inventoryComponent.Size;
        //        inventory.InventoryItems = MapItems(inventoryComponent);
        //    }
        //}
        //else
        //    userData.Inventories = new List<InventoryData>();

        //if (player.TryGetComponent(out StatisticsCounterComponent statisticsCounterComponent))
        //{
        //    userData.Stats = statisticsCounterComponent.GetStatsIds.Select(x => new UserStatData
        //    {
        //        StatId = x,
        //        Value = statisticsCounterComponent.GetStat(x)
        //    }).ToList();
        //    _dbContext.UserStats.AddRange(userData.Stats);
        //}
        //else
        //    userData.Stats = new List<UserStatData>();

        //if (player.TryGetComponent(out AchievementsComponent achievementsComponent))
        //{
        //    userData.Achievements = achievementsComponent.Achievements.Select(x => new AchievementData
        //    {
        //        AchievementId = x.Key,
        //        Value = JsonConvert.SerializeObject(x.Value.value, Formatting.None),
        //        PrizeReceived = x.Value.rewardReceived,
        //        Progress = x.Value.progress,
        //    }).ToList();
        //}
        //else
        //    userData.Achievements = new List<AchievementData>();

        //if (player.TryGetComponent(out JobUpgradesComponent jobUpgradesComponent))
        //{
        //    userData.JobUpgrades = jobUpgradesComponent.Upgrades.Select(x => new JobUpgradeData
        //    {
        //        JobId = x.jobId,
        //        UpgradeId = x.upgradeId,
        //    }).ToList();
        //}
        //else
        //    userData.JobUpgrades = new List<JobUpgradeData>();

        //if (player.TryGetComponent(out JobStatisticsComponent jobStatisticsComponent))
        //{
        //    var newStatistics = jobStatisticsComponent.JobStatistics.Where(x => x.Value.sessionPoints > 0 || x.Value.sessionTimePlayed > 0);
        //    foreach (var item in newStatistics)
        //    {
        //        var first = userData.JobStatistics.FirstOrDefault(x => x.JobId == item.Value.jobId && x.Date == jobStatisticsComponent.Date);
        //        if (first == null)
        //        {
        //            userData.JobStatistics.Add(new JobStatisticsData
        //            {
        //                Date = jobStatisticsComponent.Date,
        //                JobId = item.Key,
        //                Points = item.Value.sessionPoints,
        //                TimePlayed = item.Value.sessionTimePlayed
        //            });
        //        }
        //        else
        //        {
        //            first.Points += item.Value.sessionPoints;
        //            first.TimePlayed += item.Value.sessionTimePlayed;
        //        }
        //    }
        //    jobStatisticsComponent.Reset();
        //}
        //else
        //    userData.JobStatistics = new List<JobStatisticsData>();

        //if (player.TryGetComponent(out DiscoveriesComponent discoveriesComponent))
        //{
        //    userData.Discoveries = discoveriesComponent.Discoveries.Select(x => new DiscoveryData
        //    {
        //        DiscoveryId = x,
        //    }).ToList();
        //}

        //if (player.TryGetComponent(out LevelComponent levelComponent))
        //{
        //    userData.Level = levelComponent.Level;
        //    userData.Experience = levelComponent.Experience;
        //}
        //else
        //{
        //    userData.Level = 0;
        //    userData.Experience = 0;
        //}

        //if (player.TryGetComponent(out DiscordIntegrationComponent discordIntegrationComponent))
        //{
        //    userData.DiscordIntegration = new DiscordIntegrationData
        //    {
        //        DiscordUserId = discordIntegrationComponent.DiscordUserId
        //    };
        //}
        //else
        //{
        //    userData.DiscordIntegration = null;
        //}

        return true;
    }

    public async Task<bool> Save(Element element)
    {
        if (await BeginSave(element))
        {
            await Commit();
            return true;
        }
        return false;
    }

    public async Task<bool> BeginSave(Element element)
    {
        bool saved = element switch
        {
            RealmPlayer player => await SavePlayer(player),
            RealmVehicle vehicle => await SaveVehicle(vehicle),
            _ => false
        };

        if (saved)
            ElementSaved?.Invoke(element);
        return saved;
    }

    public async Task Commit()
    {
        await _dbContext.SaveChangesAsync();
    }
}

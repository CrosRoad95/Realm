using Realm.Domain.Components.Object;
using Realm.Domain.Components.Players;
using Realm.Domain.Interfaces;
using Realm.Domain.Registries;
using static Realm.Domain.Components.Elements.PlayerElementComponent;

namespace Realm.Console.Logic;

internal sealed class CommandsLogic
{
    private readonly RPGCommandService _commandService;
    private readonly ILogger _logger;
    private readonly IEntityFactory _entityFactory;
    private readonly RepositoryFactory _repositoryFactory;
    private readonly ItemsRegistry _itemsRegistry;
    private readonly IGroupService _groupService;

    public CommandsLogic(RPGCommandService commandService, ILogger logger, IEntityFactory entityFactory, RepositoryFactory repositoryFactory,
        ItemsRegistry itemsRegistry, IGroupService groupService)
    {
        _commandService = commandService;
        _logger = logger;
        _entityFactory = entityFactory;
        _repositoryFactory = repositoryFactory;
        _itemsRegistry = itemsRegistry;
        _groupService = groupService;
        _commandService.AddCommandHandler("gp", (entity, args) =>
        {
            logger.Information("{position}, {rotation}", entity.Transform.Position, entity.Transform.Rotation);
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("inventory", (entity, args) =>
        {
            if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
                playerElementComponent.SendChatMessage($"Inventory, {inventoryComponent.Number}/{inventoryComponent.Size}");
                foreach (var item in inventoryComponent.Items)
                {
                    playerElementComponent.SendChatMessage($"Item, {item.ItemId} = {item.Name}");
                }

            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("giveitem", (entity, args) =>
        {
            if(entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                uint itemId = uint.Parse(args.ElementAtOrDefault(0) ?? "1");
                uint count = uint.Parse(args.ElementAtOrDefault(1) ?? "1");
                inventoryComponent.AddItem(_itemsRegistry, itemId, count);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Item added, {inventoryComponent.Number}/{inventoryComponent.Size}");
            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("licenses", (entity, args) =>
        {
            if (entity.TryGetComponent(out LicensesComponent licenseComponent))
            {
                var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
                playerElementComponent.SendChatMessage($"Licenses");
                foreach (var license in licenseComponent.Licenses)
                {
                    playerElementComponent.SendChatMessage($"License: {license.licenseId} = {license.IsSuspended}");
                }

            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("givelicense", (entity, args) =>
        {
            if (entity.TryGetComponent(out LicensesComponent licenseComponent))
            {
                var license = args.First();
                if (licenseComponent.AddLicense(license))
                {
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"license added: '{license}'");
                }
                else
                {
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"failed to add license: '{license}'");
                }
            }
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("playtime", (entity, args) =>
        {
            if (entity.TryGetComponent(out PlayTimeComponent playTimeComponent))
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"playtime: {playTimeComponent.PlayTime}, total play time: {playTimeComponent.TotalPlayTime}");
            }
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("givemoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = decimal.Parse(args.First());
                moneyComponent.GiveMoney(amount);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"gave money: {amount}, total money: {moneyComponent.Money}");
            }
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("takemoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = decimal.Parse(args.First());
                moneyComponent.TakeMoney(amount);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"taken money: {amount}, total money: {moneyComponent.Money}");
            }
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("setmoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = decimal.Parse(args.First());
                moneyComponent.Money = amount;
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"taken money: {amount}, total money: {moneyComponent.Money}");
            }
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("money", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"total money: {moneyComponent.Money}");
            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("cv", (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent(new VehicleUpgradesComponent());
            vehicleEntity.AddComponent(new MileageCounterComponent());
            vehicleEntity.AddComponent(new VehicleFuelComponent("default", 20, 20, 0.01, 2)).Active = true;
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("exclusivecv", (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent(new VehicleUpgradesComponent());
            vehicleEntity.AddComponent(new MileageCounterComponent());
            vehicleEntity.AddComponent(new VehicleFuelComponent("default", 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent(new VehicleExclusiveAccessComponent(vehicleEntity));
            entity.Transform.Position = vehicleEntity.Transform.Position;
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("privateblip", async (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var blipElementComponent = _entityFactory.CreateBlipFor(entity, BlipIcon.Pizza, entity.Transform.Position);
            await Task.Delay(1000);
            entity.DestroyComponent(blipElementComponent);
        });

        _commandService.AddCommandHandler("addmeasowner", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var veh = playerElementComponent.OccupiedVehicle;
            veh.GetRequiredComponent<PrivateVehicleComponent>().AddAsOwner(entity);
        });

        _commandService.AddCommandHandler("accessinfo", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var veh = playerElementComponent.OccupiedVehicle;
            if(veh == null)
            {
                playerElementComponent.SendChatMessage("Enter vehicle!");
                return;
            }

            var privateVehicleComponent = veh.GetRequiredComponent<PrivateVehicleComponent>();
            playerElementComponent.SendChatMessage("Access info:");

            foreach (var vehicleAccess in privateVehicleComponent.VehicleAccesses)
            {
                playerElementComponent.SendChatMessage($"Access: ({vehicleAccess.UserId}) = Ownership={vehicleAccess.Ownership}");
            }
        });

        _commandService.AddCommandHandler("testachievement", async (entity, args) =>
        {
            var achievementsComponent = entity.GetRequiredComponent<AchievementsComponent>();
            achievementsComponent.UpdateProgress("test", 2, 10);
            entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"progressed achieviement 'test'");
        });

        _commandService.AddCommandHandler("addupgrade", async (entity, args) =>
        {
            var jobUpgradesComponent = entity.GetRequiredComponent<JobUpgradesComponent>();
            try
            {
                jobUpgradesComponent.AddJobUpgrade(1, "foo");
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage("Upgrade added");
            }
            catch (Exception ex)
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Failed to add upgrade: {ex.Message}");
            }
        });

        _commandService.AddCommandHandler("addvehicleupgrade", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var veh = playerElementComponent.OccupiedVehicle;
            if (veh == null)
            {
                playerElementComponent.SendChatMessage("Enter vehicle!");
                return;
            }

            var vehicleUpgradeComponent = veh.GetRequiredComponent<VehicleUpgradesComponent>();
            if(vehicleUpgradeComponent.HasUpgrade(1))
            {
                playerElementComponent.SendChatMessage("You already have a upgrade!");
                return;
            }
            vehicleUpgradeComponent.AddUpgrade(1);
            playerElementComponent.SendChatMessage("Upgrade added");
        });

        _commandService.AddCommandHandler("comps", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SendChatMessage("Components:");
            foreach (var component in entity.Components)
            {
                playerElementComponent.SendChatMessage($"> {component}");
            }
        });

        _commandService.AddCommandHandler("addtestdata", async (entity, args) =>
        {
            if (entity.TryGetComponent(out JobUpgradesComponent jobUpgradesComponent))
            {
                jobUpgradesComponent.AddJobUpgrade(1, "foo");
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Job upgrade added");
            }
            
            if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                inventoryComponent.AddItem(_itemsRegistry, 1);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Test item added");
            }

            if (entity.TryGetComponent(out LicensesComponent licenseComponent))
            {
                if (licenseComponent.AddLicense("test123"))
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Test license added: 'test123'");
            }

            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                moneyComponent.Money = (decimal)Random.Shared.NextDouble() * 1000;
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Set money to: {moneyComponent.Money}");
            }
            

            if (entity.TryGetComponent(out AchievementsComponent achievementsComponent))
            {
                achievementsComponent.UpdateProgress("test", 2, 10);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Updated achievement 'test' progress to 2");
            }

            {
                using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
                var vehicleEntity = await _entityFactory.CreateNewPrivateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
                vehicleEntity.AddComponent(new VehicleUpgradesComponent()).AddUpgrade(1);
                vehicleEntity.AddComponent(new MileageCounterComponent());
                vehicleEntity.AddComponent(new VehicleFuelComponent("default", 20, 20, 0.01, 2)).Active = true;
            }
        });

        _commandService.AddCommandHandler("giveexperience", (entity, args) =>
        {
            if (entity.TryGetComponent(out LevelComponent levelComponent))
            {
                var amount = uint.Parse(args.First());
                levelComponent.GiveExperience(amount);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"gave experience: {amount}, level: {levelComponent.Level}, experience: {levelComponent.Experience}");
            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("cvforsale", (entity, args) =>
        {
            _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), Vector3.Zero, 0, 0, null, entity =>
            {
                entity.AddComponent(new VehicleForSaleComponent(200));
            });
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("spawnbox", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(SlipeServer.Server.Enums.ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, 0), Vector3.Zero);
            objectEntity.AddComponent(new LiftableWorldObjectComponent());
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("animation", (entity, args) =>
        {
            if (Enum.TryParse<Animation>(args.FirstOrDefault(), out var animation))
            {
                try
                {
                    entity.GetRequiredComponent<PlayerElementComponent>().DoAnimation(animation);
                }
                catch (NotSupportedException)
                {
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Animation '{args.FirstOrDefault()}' is not supported");
                }
            }
            else
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Animation '{args.FirstOrDefault()}' not found.");

            return Task.CompletedTask;
        });
        _commandService.AddCommandHandler("animationasync", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            if (Enum.TryParse<Animation>(args.FirstOrDefault(), out var animation))
            {
                try
                {
                    playerElementComponent.SendChatMessage($"Started animation '{animation}'");
                    await playerElementComponent.DoAnimationAsync(animation);
                    playerElementComponent.SendChatMessage($"Finished animation '{animation}'");
                }
                catch (NotSupportedException)
                {
                    playerElementComponent.SendChatMessage($"Animation '{args.FirstOrDefault()}' is not supported");
                }
            }
            else
                playerElementComponent.SendChatMessage($"Animation '{args.FirstOrDefault()}' not found.");

        });

        _commandService.AddCommandHandler("complexanimation", async (entity, args) =>
        {
            if (Enum.TryParse<Animation>(args.FirstOrDefault(), out var animation))
            {
                try
                {
                    await entity.GetRequiredComponent<PlayerElementComponent>().DoComplexAnimationAsync(animation, true);
                }
                catch (NotSupportedException ex)
                {
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Animation '{args.FirstOrDefault()}' is not supported");
                }
            }
            else
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Animation '{args.FirstOrDefault()}' not found.");
        });

        _commandService.AddCommandHandler("creategroup", async (entity, args) =>
        {
            var name = args.FirstOrDefault("default");
            try
            {
                var group = await _groupService.CreateGroup(name, "");
                await _groupService.AddMember(group.id, entity, 100, "Leader");

                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Group: '{name}' has been created");
            }
            catch(Exception)
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Failed to create group: '{name}'");
            }
        });

        _commandService.AddCommandHandler("mygroups", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SendChatMessage("Groups:");
            foreach (var item in entity.Components.OfType<GroupMemeberComponent>())
            {
                playerElementComponent.SendChatMessage($"Group id: {item.GroupId}, rank: {item.Rank}, rank name: '{item.RankName}'");
            }
            return Task.CompletedTask;
        });
    }
}

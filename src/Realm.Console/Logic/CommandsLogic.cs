using Realm.Domain.Interfaces;
using Realm.Resources.Assets;
using Realm.Resources.Assets.Interfaces;
using System.Drawing;
using static Realm.Domain.Components.Elements.PlayerElementComponent;

namespace Realm.Console.Logic;

internal sealed class CommandsLogic
{
    private readonly RPGCommandService _commandService;
    private readonly ILogger<CommandsLogic> _logger;
    private readonly IEntityFactory _entityFactory;
    private readonly RepositoryFactory _repositoryFactory;
    private readonly ItemsRegistry _itemsRegistry;
    private readonly IGroupService _groupService;
    private readonly AssetsRegistry _assetsRegistry;
    private readonly ECS _ecs;

    public CommandsLogic(RPGCommandService commandService, ILogger<CommandsLogic> logger, IEntityFactory entityFactory, RepositoryFactory repositoryFactory,
        ItemsRegistry itemsRegistry, IGroupService groupService, AssetsRegistry assetsRegistry, ECS ecs)
    {
        _commandService = commandService;
        _logger = logger;
        _entityFactory = entityFactory;
        _repositoryFactory = repositoryFactory;
        _itemsRegistry = itemsRegistry;
        _groupService = groupService;
        _assetsRegistry = assetsRegistry;
        _ecs = ecs;
        _commandService.AddCommandHandler("gp", (entity, args) =>
        {
            logger.LogInformation("{position}, {rotation}", entity.Transform.Position, entity.Transform.Rotation);
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
        
        _commandService.AddCommandHandler("takeitem", (entity, args) =>
        {
            if(entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                uint itemId = uint.Parse(args.ElementAtOrDefault(0) ?? "1");
                uint count = uint.Parse(args.ElementAtOrDefault(1) ?? "1");
                inventoryComponent.RemoveItem(itemId);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Item removed, {inventoryComponent.Number}/{inventoryComponent.Size}");
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
                if (licenseComponent.AddLicense(int.Parse(license)))
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
            vehicleEntity.AddComponent<VehicleUpgradesComponent>();
            vehicleEntity.AddComponent<MileageCounterComponent>();
            vehicleEntity.AddComponent(new VehicleFuelComponent("default", 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent<VehicleFocusableComponent>();
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("exclusivecv", (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent<VehicleUpgradesComponent>();
            vehicleEntity.AddComponent<MileageCounterComponent>();
            vehicleEntity.AddComponent(new VehicleFuelComponent("default", 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent(new VehicleExclusiveAccessComponent(vehicleEntity));
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("noaccesscv", (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent<VehicleUpgradesComponent>();
            vehicleEntity.AddComponent<MileageCounterComponent>();
            vehicleEntity.AddComponent(new VehicleFuelComponent("default", 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent<VehicleNoAccessComponent>();
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
                if(jobUpgradesComponent.TryAddJobUpgrade(1, 1))
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage("Upgrade added");
                else
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage("Failed to add upgrade");
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
                if (jobUpgradesComponent.TryAddJobUpgrade(1, 1))
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage("Upgrade added");
                else
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage("Failed to add upgrade");
            }
            
            if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                inventoryComponent.AddItem(_itemsRegistry, 1);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Test item added");
            }

            if (entity.TryGetComponent(out LicensesComponent licenseComponent))
            {
                if (licenseComponent.AddLicense(1))
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Test license added: 'test123' of id 1");
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
            _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), Vector3.Zero, entityBuilder: entity =>
            {
                entity.AddComponent(new VehicleForSaleComponent(200));
            });
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("spawnbox", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(SlipeServer.Server.Enums.ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            objectEntity.AddComponent(new LiftableWorldObjectComponent());
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("hud3d", (entity, args) =>
        {
            var e = _ecs.CreateEntity(Guid.NewGuid().ToString(), Entity.EntityTag.Unknown);
            e.Transform.Position = entity.Transform.Position + new Vector3(4, 0, 0);
            e.AddComponent(new Hud3dComponent<object>(e => e.AddRectangle(Vector2.Zero, new Size(50, 50), Color.Red), new object()));

            var e2 = _ecs.CreateEntity(Guid.NewGuid().ToString(), Entity.EntityTag.Unknown);
            e2.Transform.Position = entity.Transform.Position + new Vector3(-4, 0, 0);
            e2.AddComponent(new Hud3dComponent<object>(e => e
                .AddRectangle(Vector2.Zero, new Size(100, 100), Color.Red)
                .AddRectangle(new Vector2(25, 25), new Size(50, 50), Color.Green)
                , new object()));
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("spawnbox2", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(SlipeServer.Server.Enums.ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            objectEntity.AddComponent<DurationBasedHoldInteractionComponent>();
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
                catch (NotSupportedException)
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
            foreach (var item in entity.Components.OfType<GroupMemberComponent>())
            {
                playerElementComponent.SendChatMessage($"Group id: {item.GroupId}, rank: {item.Rank}, rank name: '{item.RankName}'");
            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("myfractions", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SendChatMessage("Fractions:");
            foreach (var item in entity.Components.OfType<FractionMemberComponent>())
            {
                playerElementComponent.SendChatMessage($"Fraction id: {item.FractionId}, rank: {item.Rank}, rank name: '{item.RankName}'");
            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("toggleadmindebug", (entity, args) =>
        {
            var adminComponent = entity.GetRequiredComponent<AdminComponent>();
            adminComponent.AdminTools = !adminComponent.AdminTools;
            adminComponent.InterfactionDebugRenderingEnabled = !adminComponent.InterfactionDebugRenderingEnabled;
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("createhud", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var hud = playerElementComponent.CreateHud("testhud", x => x
                .AddRectangle(new Vector2(x.Right - 400, 600), new Size(400, 20), Color.DarkBlue)
                .AddText("foo bar", new Vector2(x.Right - 200, 600), new Size(200, 20), font: "default", alignX: "center", alignY: "center")
                .AddText("custom font", new Vector2(x.Right - 400, 600), new Size(200, 20), font: _assetsRegistry.GetAsset<IFont>("Better Together.otf"), alignX: "center", alignY: "center"));
            hud.SetVisible(true);
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("movehud", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var testhud = playerElementComponent.GetHud<object>("testhud");
            testhud.Position = new Vector2(0, hudPosition++ * 10);
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("createstatefulhud", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var hud = playerElementComponent.CreateHud("teststatefulhud", x => x
                .AddRectangle(new Vector2(x.Right - 400, 600), new Size(400, 20), Color.DarkBlue)
                .AddText(x => x.Text1, new Vector2(x.Right - 200, 600), new Size(200, 20), font: "default", alignX: "center", alignY: "center")
                .AddText("custom font", new Vector2(x.Right - 400, 600), new Size(200, 20), font: _assetsRegistry.GetAsset<IFont>("Better Together.otf"), alignX: "center", alignY: "center"),
                new SampleHudState
                {
                    Text1 = "text1",
                    Text2 = "text2",
                });
            hud.SetVisible(true);
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("updatestate", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var testhud = playerElementComponent.GetHud<SampleHudState>("teststatefulhud");
            testhud.UpdateState(x =>
            {
                x.Text1 = Guid.NewGuid().ToString()[..8];
            });
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("discord", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            if (entity.HasComponent<DiscordIntegrationComponent>())
            {
                playerElementComponent.SendChatMessage("Twoje konto jest już połączone z discordem.");
                return Task.CompletedTask;
            }
            entity.TryDestroyComponent<PendingDiscordIntegrationComponent>();
            var pendingDiscordIntegrationComponent = new PendingDiscordIntegrationComponent();
            var code = pendingDiscordIntegrationComponent.GenerateAndGetDiscordConnectionCode();
            entity.AddComponent(pendingDiscordIntegrationComponent);
            playerElementComponent.SendChatMessage($"Aby połączyć konto wpisz na kanale discord #polacz-konto komendę: /polaczkonto {code}");
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("adduserupgrade", (entity, args) =>
        {
            var accountComponent = entity.GetRequiredComponent<AccountComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var i = Random.Shared.Next(0, 10);
            if (accountComponent.TryAddUpgrade(i))
                playerElementComponent.SendChatMessage($"Pomyślnie dodano ulepszenie id {i}");
            else
                playerElementComponent.SendChatMessage($"Pomyślnie dodano ulepszenie id {i}");
            return Task.CompletedTask;
        });

    }
    class SampleHudState
    {
        public string Text1 { get; set; }
        public string Text2 { get; set; }
    }

    static int hudPosition = 0;
}

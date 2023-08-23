using System.Diagnostics;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;
using RealmCore.Console.Components.Vehicles;
using RealmCore.Server.Components.Peds;
using SlipeServer.Server.Enums;
using RealmCore.Server.Components.World;
using RealmCore.Server.Enums;
using RealmCore.Server.Extensions.Resources;
using RealmCore.Console.Components.Huds;
using RealmCore.Resources.Nametags;
using RealmCore.Console.Components;
using RealmCore.Resources.ElementOutline;
using RealmCore.Resources.Assets.Factories;
using RealmCore.Resources.Assets.Interfaces;
using RealmCore.Server.Concepts.Spawning;
using RealmCore.Server.Components.Vehicles.Access;
using RealmCore.Persistence;

namespace RealmCore.Console.Logic;

internal sealed class CommandsLogic
{
    private readonly RealmCommandService _commandService;
    private readonly IEntityFactory _entityFactory;
    private readonly RepositoryFactory _repositoryFactory;
    private readonly ItemsRegistry _itemsRegistry;
    private readonly IECS _ecs;
    private readonly IBanService _banService;
    private readonly ChatBox _chatBox;
    private readonly ILogger<CommandsLogic> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IVehiclesService _vehiclesService;
    private readonly ILoadService _loadService;

    private class TestState
    {
        public string Text { get; set; }
    }

    private enum TestEnum
    {
        Test1,
        Test2,
    }

    public CommandsLogic(RealmCommandService commandService, IEntityFactory entityFactory, RepositoryFactory repositoryFactory,
        ItemsRegistry itemsRegistry, IECS ecs, IBanService banService, ChatBox chatBox, ILogger<CommandsLogic> logger,
        IDateTimeProvider dateTimeProvider, INametagsService nametagsService, IUsersService userManager, IVehiclesService vehiclesService,
        GameWorld gameWorld, IElementOutlineService elementOutlineService, IAssetsService assetsService, ISpawnMarkersService spawnMarkersService, ILoadService loadService, IFeedbackService feedbackService)
    {
        _commandService = commandService;
        _entityFactory = entityFactory;
        _repositoryFactory = repositoryFactory;
        _itemsRegistry = itemsRegistry;
        _ecs = ecs;
        _banService = banService;
        _chatBox = chatBox;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _vehiclesService = vehiclesService;
        _loadService = loadService;
        _commandService.AddCommandHandler("playtime", (entity, args) =>
        {
            if (entity.TryGetComponent(out PlayTimeComponent playTimeComponent))
            {
                _chatBox.OutputTo(entity, $"playtime: {playTimeComponent.PlayTime}, total play time: {playTimeComponent.TotalPlayTime}");
            }
        });

        _commandService.AddCommandHandler("givemoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = args.ReadDecimal();
                moneyComponent.GiveMoney(amount);
                _chatBox.OutputTo(entity, $"gave money: {amount}, total money: {moneyComponent.Money}");
            }
        });

        _commandService.AddCommandHandler("takemoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = args.ReadDecimal();
                moneyComponent.TakeMoney(amount);
                _chatBox.OutputTo(entity, $"taken money: {amount}, total money: {moneyComponent.Money}");
            }
        });

        _commandService.AddCommandHandler("setmoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = args.ReadDecimal();
                moneyComponent.Money = amount;
                _chatBox.OutputTo(entity, $"taken money: {amount}, total money: {moneyComponent.Money}");
            }
        });

        _commandService.AddCommandHandler("money", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                _chatBox.OutputTo(entity, $"total money: {moneyComponent.Money}");
            }
        });

        _commandService.AddCommandHandler("cv", (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = _entityFactory.CreateVehicle(args.ReadUShort(), entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent<VehicleUpgradesComponent>();
            vehicleEntity.AddComponent<MileageCounterComponent>();
            vehicleEntity.AddComponent(new FuelComponent(1, 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent<FocusableComponent>();
            vehicleEntity.AddComponent<VehiclePartDamageComponent>().AddPart(1, 1337);
        });

        _commandService.AddAsyncCommandHandler("cvprivate", async (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = await _entityFactory.CreateNewPrivateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent(new VehicleUpgradesComponent()).AddUpgrade(1);
            vehicleEntity.AddComponent<MileageCounterComponent>();
            vehicleEntity.AddComponent<VehicleEngineComponent>();
            vehicleEntity.AddComponent(new FuelComponent(1, 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent<VehiclePartDamageComponent>().AddPart(1, 1337);
            vehicleEntity.GetRequiredComponent<PrivateVehicleComponent>().Access.AddAsOwner(entity);
        });

        _commandService.AddCommandHandler("exclusivecv", (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent<VehicleUpgradesComponent>();
            vehicleEntity.AddComponent<MileageCounterComponent>();
            vehicleEntity.AddComponent(new FuelComponent(1, 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent(new VehicleExclusiveAccessComponent(entity));
        });

        _commandService.AddCommandHandler("noaccesscv", (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent<VehicleUpgradesComponent>();
            vehicleEntity.AddComponent<MileageCounterComponent>();
            vehicleEntity.AddComponent(new FuelComponent(1, 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent<VehicleNoAccessComponent>();
        });

        _commandService.AddAsyncCommandHandler("privateblip", async (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var blipElementComponent = _entityFactory.CreateBlipFor(entity, BlipIcon.Pizza, entity.Transform.Position);
            await Task.Delay(1000);
            entity.DestroyComponent(blipElementComponent);
        });

        _commandService.AddCommandHandler("addmeasowner", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var veh = playerElementComponent.OccupiedVehicle;
            veh.GetRequiredComponent<PrivateVehicleComponent>().Access.AddAsOwner(entity);
        });

        _commandService.AddCommandHandler("accessinfo", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var veh = playerElementComponent.OccupiedVehicle;
            if (veh == null)
            {
                _chatBox.OutputTo(entity, "Enter vehicle!");
                return;
            }

            var privateVehicleComponent = veh.GetRequiredComponent<PrivateVehicleComponent>();
            _chatBox.OutputTo(entity, "Access info:");

            foreach (var vehicleAccess in privateVehicleComponent.Access.PlayerAccesses)
            {
                _chatBox.OutputTo(entity, $"Access: ({vehicleAccess.UserId}) = Ownership={vehicleAccess.AccessType == 0}");
            }
        });

        _commandService.AddAsyncCommandHandler("accessinfobyid", async (entity, args) =>
        {
            using var access = await vehiclesService.GetVehicleAccess(args.ReadInt());
            if (access == null)
            {
                _chatBox.OutputTo(entity, "Vehicle not found");
                return;
            }


            _chatBox.OutputTo(entity, "Access info:");

            foreach (var vehicleAccess in access.PlayerAccesses)
            {
                _chatBox.OutputTo(entity, $"Access: ({vehicleAccess.UserId}) = Ownership={vehicleAccess.AccessType == 0}");
            }
        });

        _commandService.AddCommandHandler("testachievement", (entity, args) =>
        {
            var achievementsComponent = entity.GetRequiredComponent<AchievementsComponent>();
            achievementsComponent.UpdateProgress(1, 2, 10);
            _chatBox.OutputTo(entity, $"progressed achieviement 'test'");
        });

        _commandService.AddCommandHandler("addupgrade", (entity, args) =>
        {
            var jobUpgradesComponent = entity.GetRequiredComponent<JobUpgradesComponent>();
            try
            {
                if (jobUpgradesComponent.TryAddJobUpgrade(1, 1))
                    _chatBox.OutputTo(entity, "Upgrade added");
                else
                    _chatBox.OutputTo(entity, "Failed to add upgrade");
            }
            catch (Exception ex)
            {
                _chatBox.OutputTo(entity, $"Failed to add upgrade: {ex.Message}");
            }
        });

        _commandService.AddCommandHandler("addvehicleupgrade", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var veh = playerElementComponent.OccupiedVehicle;
            if (veh == null)
            {
                _chatBox.OutputTo(entity, "Enter vehicle!");
                return;
            }

            var vehicleUpgradeComponent = veh.GetRequiredComponent<VehicleUpgradesComponent>();
            if (vehicleUpgradeComponent.HasUpgrade(1))
            {
                _chatBox.OutputTo(entity, "You already have a upgrade!");
                return;
            }
            vehicleUpgradeComponent.AddUpgrade(1);
            _chatBox.OutputTo(entity, "Upgrade added");
        });

        _commandService.AddCommandHandler("comps", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _chatBox.OutputTo(entity, "Components:");
            foreach (var component in entity.Components)
            {
                _chatBox.OutputTo(entity, $"> {component}");
            }
        });

        _commandService.AddCommandHandler("additem", (entity, args) =>
        {
            if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                inventoryComponent.AddItem(_itemsRegistry, 1);
                _chatBox.OutputTo(entity, $"Test item added");
            }
        });
        _commandService.AddCommandHandler("additem2", (entity, args) =>
        {
            if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                inventoryComponent.AddItem(_itemsRegistry, 2);
                _chatBox.OutputTo(entity, $"Test item added");
            }
        });
        _commandService.AddAsyncCommandHandler("addtestdata", async (entity, args) =>
        {
            if (entity.TryGetComponent(out JobUpgradesComponent jobUpgradesComponent))
            {
                if (jobUpgradesComponent.TryAddJobUpgrade(1, 1))
                    _chatBox.OutputTo(entity, "Upgrade added");
                else
                    _chatBox.OutputTo(entity, "Failed to add upgrade");
            }

            if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                inventoryComponent.AddItem(_itemsRegistry, 1);
                _chatBox.OutputTo(entity, $"Test item added");
            }

            if (entity.TryGetComponent(out LicensesComponent licenseComponent))
            {
                if (licenseComponent.TryAddLicense(1))
                    _chatBox.OutputTo(entity, $"Test license added: 'test123' of id 1");
            }

            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                moneyComponent.Money = (decimal)Random.Shared.NextDouble() * 1000;
                _chatBox.OutputTo(entity, $"Set money to: {moneyComponent.Money}");
            }


            if (entity.TryGetComponent(out AchievementsComponent achievementsComponent))
            {
                achievementsComponent.UpdateProgress(1, 2, 10);
                _chatBox.OutputTo(entity, $"Updated achievement 'test' progress to 2");
            }

            {
                using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
                var vehicleEntity = await _entityFactory.CreateNewPrivateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
                vehicleEntity.AddComponent(new VehicleUpgradesComponent()).AddUpgrade(1);
                vehicleEntity.AddComponent(new MileageCounterComponent());
                vehicleEntity.AddComponent(new FuelComponent(1, 20, 20, 0.01, 2)).Active = true;
                vehicleEntity.AddComponent<VehiclePartDamageComponent>().AddPart(1, 1337);
            }
        });

        _commandService.AddCommandHandler("giveexperience", (entity, args) =>
        {
            if (entity.TryGetComponent(out LevelComponent levelComponent))
            {
                var amount = args.ReadUInt();
                levelComponent.GiveExperience(amount);
                _chatBox.OutputTo(entity, $"gave experience: {amount}, level: {levelComponent.Level}, experience: {levelComponent.Experience}");
            }
        });

        _commandService.AddCommandHandler("cvforsale", (entity, args) =>
        {
            _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), Vector3.Zero, entityBuilder: entity =>
            {
                entity.AddComponent(new VehicleForSaleComponent(200));
            });
        });

        _commandService.AddAsyncCommandHandler("privateoutlinetest", async (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            await Task.Delay(2000);
            elementOutlineService.SetEntityOutlineForPlayer(entity, objectEntity, Color.Red);
            await Task.Delay(1000);
            elementOutlineService.RemoveEntityOutlineForPlayer(entity, objectEntity);
            _chatBox.OutputTo(entity, "removed");
        });

        _commandService.AddCommandHandler("spawnbox", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            objectEntity.AddComponent<LiftableWorldObjectComponent>();
        });

        _commandService.AddCommandHandler("spawnmybox", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            objectEntity.AddComponent<LiftableWorldObjectComponent>();
            objectEntity.AddComponent(new OwnerComponent(entity));
        });

        _commandService.AddCommandHandler("spawnmybox2", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            objectEntity.AddComponent<LiftableWorldObjectComponent>();
            objectEntity.AddComponent(new OwnerDisposableComponent(entity));
        });

        _commandService.AddCommandHandler("spawnboxforme", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObjectVisibleFor(entity, ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            objectEntity.AddComponent<LiftableWorldObjectComponent>();
        });

        _commandService.AddAsyncCommandHandler("spawntempbox", async (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            objectEntity.AddComponent(new LiftableWorldObjectComponent());
            await Task.Delay(5000);
            objectEntity.Dispose();
        });

        _commandService.AddCommandHandler("spawnboard", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject((ObjectModel)3077, entity.Transform.Position + new Vector3(4, 0, -1), Vector3.Zero);
            objectEntity.AddComponent(new LiftableWorldObjectComponent());
        });

        _commandService.AddCommandHandler("spawnboxmany", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.6f), Vector3.Zero);
            objectEntity.AddComponent(new LiftableWorldObjectComponent());
            var objectEntity1 = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4 + 0.8f, 0, -0.6f), Vector3.Zero);
            objectEntity1.AddComponent(new LiftableWorldObjectComponent());
            var objectEntity2 = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0 + 0.8f, -0.6f), Vector3.Zero);
            objectEntity2.AddComponent(new LiftableWorldObjectComponent());
            var objectEntity3 = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4 + 0.8f, 0 + 0.8f, -0.6f), Vector3.Zero);
            objectEntity3.AddComponent(new LiftableWorldObjectComponent());
        });

        _commandService.AddAsyncCommandHandler("hud3d", async (entity, args) =>
        {
            using var e = _ecs.CreateEntity(Guid.NewGuid().ToString());
            e.Transform.Position = entity.Transform.Position + new Vector3(-4, 0, 0);
            e.AddComponent(new Hud3dComponent<TestState>(e => e
                .AddRectangle(Vector2.Zero, new Size(100, 100), Color.Red)
                .AddRectangle(new Vector2(25, 25), new Size(50, 50), Color.Green)
                .AddText(x => x.Text, new Vector2(0, 0), new Size(100, 100), Color.Blue)
                , new TestState
                {
                    Text = "test 1",
                }));

            await Task.Delay(2000);
        });

        _commandService.AddAsyncCommandHandler("hud3d2", async (entity, args) =>
        {
            var e = _ecs.CreateEntity(Guid.NewGuid().ToString());
            e.Transform.Position = entity.Transform.Position + new Vector3(-4, 0, 0);
            var hud3d = e.AddComponent(new Hud3dComponent<TestState>(e => e
                .AddRectangle(Vector2.Zero, new Size(200, 200), Color.Red)
                .AddRectangle(new Vector2(25, 25), new Size(50, 50), Color.Green)
                .AddText(x => x.Text, new Vector2(0, 0), new Size(200, 200), Color.White)
                , new TestState
                {
                    Text = "test 1",
                }));

            int i = 0;
            while (true)
            {
                await Task.Delay(1000 / 60);
                hud3d.UpdateState(x => x.Text = $"time {_dateTimeProvider.Now} {i++}");
            }
        });

        _commandService.AddCommandHandler("spawnbox2", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            objectEntity.AddComponent<DurationBasedHoldInteractionWithRingEffectComponent>();
        });

        _commandService.AddCommandHandler("stopanimation", (entity, args) =>
        {
            entity.GetRequiredComponent<PlayerElementComponent>().StopAnimation();
        });

        _commandService.AddCommandHandler("animation", (entity, args) =>
        {
            var animationName = args.ReadArgument();
            if (Enum.TryParse<Animation>(animationName, out var animation))
            {
                try
                {
                    entity.GetRequiredComponent<PlayerElementComponent>().DoAnimation(animation);
                }
                catch (NotSupportedException)
                {
                    _chatBox.OutputTo(entity, $"Animation '{animationName}' is not supported");
                }
            }
            else
                _chatBox.OutputTo(entity, $"Animation '{animationName}' not found.");
        });

        _commandService.AddAsyncCommandHandler("animationasync", async (entity, args) =>
        {
            var animationName = args.ReadArgument();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            if (Enum.TryParse<Animation>(animationName, out var animation))
            {
                try
                {
                    _chatBox.OutputTo(entity, $"Started animation '{animation}'");
                    await playerElementComponent.DoAnimationAsync(animation);
                    _chatBox.OutputTo(entity, $"Finished animation '{animation}'");
                }
                catch (NotSupportedException)
                {
                    _chatBox.OutputTo(entity, $"Animation '{animationName}' is not supported");
                }
            }
            else
                _chatBox.OutputTo(entity, $"Animation '{animationName}' not found.");

        });

        _commandService.AddAsyncCommandHandler("complexanimation", async (entity, args) =>
        {
            var animationName = args.ReadArgument();
            if (Enum.TryParse<Animation>(animationName, out var animation))
            {
                try
                {
                    await entity.GetRequiredComponent<PlayerElementComponent>().DoComplexAnimationAsync(animation, true);
                }
                catch (NotSupportedException)
                {
                    _chatBox.OutputTo(entity, $"Animation '{animationName}' is not supported");
                }
            }
            else
                _chatBox.OutputTo(entity, $"Animation '{animationName}' not found.");
        });

        _commandService.AddCommandHandler("mygroups", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _chatBox.OutputTo(entity, "Groups:");
            foreach (var item in entity.Components.OfType<GroupMemberComponent>())
            {
                _chatBox.OutputTo(entity, $"Group id: {item.GroupId}, rank: {item.Rank}, rank name: '{item.RankName}'");
            }
        });

        _commandService.AddCommandHandler("myfractions", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _chatBox.OutputTo(entity, "Fractions:");
            foreach (var item in entity.Components.OfType<FractionMemberComponent>())
            {
                _chatBox.OutputTo(entity, $"Fraction id: {item.FractionId}, rank: {item.Rank}, rank name: '{item.RankName}'");
            }
        });

        _commandService.AddCommandHandler("toggleadmindebug", (entity, args) =>
        {
            var adminComponent = entity.GetRequiredComponent<AdminComponent>();
            adminComponent.AdminMode = !adminComponent.AdminMode;
            adminComponent.InteractionDebugRenderingEnabled = !adminComponent.InteractionDebugRenderingEnabled;
        });

        _commandService.AddCommandHandler("createvehiclehud", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            entity.AddComponent(new SampleVehicleHud());
        });

        _commandService.AddCommandHandler("createhud", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            entity.AddComponent(new SampleHud());
        });

        _commandService.AddCommandHandler("createhud2", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            entity.AddComponent(new SampleHud2());
        });

        _commandService.AddCommandHandler("updatestate2", (entity, args) =>
        {
            var sampleHud2 = entity.GetRequiredComponent<SampleHud2>();
            sampleHud2.Update();
        });

        _commandService.AddCommandHandler("movehud", (entity, args) =>
        {
            var sampleHud = entity.GetRequiredComponent<SampleHud>();
            sampleHud.Position = new Vector2(0, _hudPosition++ * 10);
        });

        _commandService.AddCommandHandler("createstatefulhud", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var sampleHud = entity.AddComponent(new SampleStatefulHud(new SampleHudState
            {
                Text1 = "text1",
                Text2 = "text2",
            }));
        });

        _commandService.AddCommandHandler("updatestate", (entity, args) =>
        {
            var sampleHud = entity.GetRequiredComponent<SampleStatefulHud>();
            sampleHud.Update();
        });

        _commandService.AddCommandHandler("destroyhuds", (entity, args) =>
        {
            entity.TryDestroyComponent<SampleHud>();
            entity.TryDestroyComponent<SampleStatefulHud>();
        });

        _commandService.AddCommandHandler("discord", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            if (entity.HasComponent<DiscordIntegrationComponent>())
            {
                _chatBox.OutputTo(entity, "Twoje konto jest już połączone z discordem.");
            }
            entity.TryDestroyComponent<PendingDiscordIntegrationComponent>();
            var pendingDiscordIntegrationComponent = new PendingDiscordIntegrationComponent();
            var code = pendingDiscordIntegrationComponent.GenerateAndGetDiscordConnectionCode();
            entity.AddComponent(pendingDiscordIntegrationComponent);
            _chatBox.OutputTo(entity, $"Aby połączyć konto wpisz na kanale discord #polacz-konto komendę: /polaczkonto {code}");
        });

        _commandService.AddCommandHandler("adduserupgrade", (entity, args) =>
        {
            var userComponent = entity.GetRequiredComponent<UserComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var i = Random.Shared.Next(0, 10);
            if (userComponent.TryAddUpgrade(i))
                _chatBox.OutputTo(entity, $"Pomyślnie dodano ulepszenie id {i}");
            else
                _chatBox.OutputTo(entity, $"Pomyślnie dodano ulepszenie id {i}");
        });

        _commandService.AddAsyncCommandHandler("ban", async (entity, args) =>
        {
            var userComponent = entity.GetRequiredComponent<UserComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            await _banService.BanUserId(userComponent.Id);
            playerElementComponent.Kick("test 123");
        });

        _commandService.AddCommandHandler("attach", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject((ObjectModel)1337, Vector3.Zero, Vector3.Zero);
            //entity.AddComponent(new AttachedEntityComponent(objectEntity, SlipeServer.Packets.Enums.BoneId.Pelvis1, new Vector3(-1, -1, 1)));
            entity.AddComponent(new OwnerDisposableComponent(objectEntity));
            objectEntity.Disposed += e =>
            {
                logger.LogInformation("Disposed attached entity");
            };
        });

        _commandService.AddCommandHandler("destroyattachedentity", (entity, args) =>
        {
            var userComponent = entity.GetRequiredComponent<UserComponent>();
            var attachedEntity = entity.GetRequiredComponent<AttachedEntityComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            attachedEntity.AttachedEntity.Dispose();
            if (entity.HasComponent<AttachedEntityComponent>())
            {
                _chatBox.OutputTo(entity, "Nie udalo sie zniszczyc");
            }
            else
            {
                _chatBox.OutputTo(entity, "Zniszczone");
            }
        });

        _commandService.AddCommandHandler("testrole", (entity, args) =>
        {
            var userComponent = entity.GetRequiredComponent<UserComponent>();
            var isAdmin = userComponent.IsInRole("admin");
            var roles = userComponent.GetRoles();
            foreach (var item in roles)
            {
                if (!userComponent.IsInRole(item))
                {
                    throw new Exception();
                }
            }
        });

        _commandService.AddCommandHandler("testlogs", (entity, args) =>
        {
            _logger.LogInformation("test test 1");
            var activity = new Activity("TestLogsActivity");
            activity.Start();
            _logger.LogInformation("test test 2");
            activity.Stop();
        });

        _commandService.AddCommandHandler("nametags", (entity, args) =>
        {
            var ped = _entityFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, entity.Transform.Position + new Vector3(4, 0, 0));
            ped.AddComponent(new NametagComponent("[22] Borsuk"));
        });

        _commandService.AddAsyncCommandHandler("nametags2", async (entity, args) =>
        {
            var nametag = new NametagComponent("[22] Borsuk");
            var ped = _entityFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, entity.Transform.Position + new Vector3(4, 0, 0));
            ped.AddComponent(nametag);
            await Task.Delay(1000);
            ped.DestroyComponent(nametag);
        });

        _commandService.AddAsyncCommandHandler("nametags3", async (entity, args) =>
        {
            var nametag = new NametagComponent("[22] Borsuk");
            var ped = _entityFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, entity.Transform.Position + new Vector3(4, 0, 0));
            ped.AddComponent(nametag);
            await Task.Delay(1000);
            nametag.Text = "[100] Borsuk";
        });

        _commandService.AddCommandHandler("nametags4", (entity, args) =>
        {
            nametagsService.SetNametagRenderingEnabled(entity, args.ReadArgument() == "true");
        });

        _commandService.AddAsyncCommandHandler("nametags5", async (entity, args) =>
        {
            var nametag = new NametagComponent("[22] Borsuk");
            using var ped = _entityFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, entity.Transform.Position + new Vector3(4, 0, 0));
            ped.AddComponent(nametag);
            await Task.Delay(1000);
        });

        _commandService.AddCommandHandler("nametags6", (entity, args) =>
        {
            nametagsService.SetLocalPlayerRenderingEnabled(entity, args.ReadArgument() == "true");
        });

        _commandService.AddCommandHandler("outlinerendering", (entity, args) =>
        {
            elementOutlineService.SetRenderingEnabled(entity, args.ReadArgument() == "true");
        });

        _commandService.AddCommandHandler("outline1", (entity, args) =>
        {
            var ped = _entityFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, entity.Transform.Position + new Vector3(4, 0, 0));
            var @object = _entityFactory.CreateObject((ObjectModel)1337, entity.Transform.Position + new Vector3(4, 4, 0), Vector3.Zero);
            ped.AddComponent(new OutlineComponent(Color.Red));
            @object.AddComponent(new OutlineComponent(Color.Red));
        });

        _commandService.AddAsyncCommandHandler("outline2", async (entity, args) =>
        {
            using var ped = _entityFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, entity.Transform.Position + new Vector3(4, 0, 0));
            ped.AddComponent(new OutlineComponent(Color.Red));
            await Task.Delay(1000);
        });

        _commandService.AddAsyncCommandHandler("outline3", async (entity, args) =>
        {
            var ped = _entityFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, entity.Transform.Position + new Vector3(4, 0, 0));
            using var outlineComponent = ped.AddComponent(new OutlineComponent(Color.Red));
            await Task.Delay(1000);
        });

        _commandService.AddCommandHandler("outline4", (entity, args) =>
        {
            var ped = _entityFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, entity.Transform.Position + new Vector3(4, 0, 0));
            var @object = _entityFactory.CreateObject((ObjectModel)1337, entity.Transform.Position + new Vector3(4, 4, 0), Vector3.Zero);
            elementOutlineService.SetEntityOutlineForPlayer(entity, ped, Color.Red);
            elementOutlineService.SetEntityOutlineForPlayer(entity, @object, Color.Blue);
        });

        _commandService.AddCommandHandler("randomvehcolor", (entity, args) =>
        {
            var rnd = Random.Shared;
            var veh = entity.GetRequiredComponent<PlayerElementComponent>().OccupiedVehicle.GetRequiredComponent<VehicleElementComponent>();
            veh.PrimaryColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            veh.SecondaryColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            veh.Color3 = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            veh.Color4 = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        });

        _commandService.AddCommandHandler("setsetting", (entity, args) =>
        {
            entity.GetRequiredComponent<UserComponent>().SetSetting(1, args.ReadArgument());
        });

        _commandService.AddCommandHandler("removesetting", (entity, args) =>
        {
            entity.GetRequiredComponent<UserComponent>().RemoveSetting(1);
        });

        _commandService.AddCommandHandler("getsetting", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();

            var settingValue = entity.GetRequiredComponent<UserComponent>().GetSetting(1);

            _chatBox.OutputTo(entity, $"Setting1: {settingValue}");
        });


        _commandService.AddAsyncCommandHandler("whitelistmyserial", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var userComponent = entity.GetRequiredComponent<UserComponent>();

            if (await userManager.TryAddWhitelistedSerial(userComponent.Id, playerElementComponent.Client.Serial))
            {
                _chatBox.OutputTo(entity, $"Dodano serial");
            }
            else
            {
                _chatBox.OutputTo(entity, $"Nie udało się dodać");
            }
        });

        _commandService.AddAsyncCommandHandler("removewhitelistmyserial", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var userComponent = entity.GetRequiredComponent<UserComponent>();

            if (await userManager.TryRemoveWhitelistedSerial(userComponent.Id, playerElementComponent.Client.Serial))
            {
                _chatBox.OutputTo(entity, $"Usunięto serial");
            }
            else
            {
                _chatBox.OutputTo(entity, $"Nie udało się usunąć");
            }
        });

        _commandService.AddCommandHandler("addvisualupgrade", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            if (playerElementComponent.OccupiedVehicle.GetRequiredComponent<VehicleUpgradesComponent>().AddUniqueUpgrade(3))
            {
                _chatBox.OutputTo(entity, $"dodano wizualne ulepszenie");
            }
            else
            {
                _chatBox.OutputTo(entity, $"Nie udało się dodać wizualnego ulepszenia");
            }
        });

        _commandService.AddCommandHandler("addinvalidguicomponent", (entity, args) =>
        {
            entity.AddComponent<InvalidGuiComponent>();
        });

        _commandService.AddCommandHandler("text3dcomp", (entity, args) =>
        {
            var markerEntity = entityFactory.CreateMarker(MarkerType.Arrow, entity.Transform.Position);
            markerEntity.AddComponent(new Text3dComponent("test1"));
            markerEntity.AddComponent(new Text3dComponent("offset z+1", new Vector3(0, 0, 1)));
        });

        _commandService.AddAsyncCommandHandler("despawn", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            await _vehiclesService.Destroy(playerElementComponent.OccupiedVehicle);
        });

        _commandService.AddCommandHandler("disposeveh", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.OccupiedVehicle.Dispose();
        });

        _commandService.AddAsyncCommandHandler("spawnback", async (entity, args) =>
        {
            var en = await _loadService.LoadVehicleById(args.ReadInt());
            await _vehiclesService.SetVehicleSpawned(en);
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            if (en != null)
            {
                _chatBox.OutputTo(entity, "Spawned");
            }
            else
                _chatBox.OutputTo(entity, "Error while spawning");
        });

        _commandService.AddCommandHandler("inventoryoccupied", (entity, args) =>
        {
            var inv = entity.GetRequiredComponent<InventoryComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _chatBox.OutputTo(entity, $"Inventory: {inv.Number}/{inv.Size}");
        });

        _commandService.AddCommandHandler("giveitem4", (entity, args) =>
        {
            var inv = entity.GetRequiredComponent<InventoryComponent>();
            inv.AddSingleItem(_itemsRegistry, 4);
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _chatBox.OutputTo(entity, "Item given");
        });

        _commandService.AddCommandHandler("itemwithmetadata", (entity, args) =>
        {
            var inv = entity.GetRequiredComponent<InventoryComponent>();
            var item = inv.AddSingleItem(_itemsRegistry, 4, new Dictionary<string, object>
            {
                ["number"] = 1m
            });

            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _chatBox.OutputTo(entity, $"Item regular: {item.GetMetadata("number").GetType()}");
            _chatBox.OutputTo(entity, $"Item cast<int>: {item.GetMetadata<int>("number").GetType()}");
        });

        _commandService.AddCommandHandler("proceduralobject", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
        });

        _commandService.AddCommandHandler("day", (entity, args) =>
        {
            gameWorld.SetTime(12, 0);
        });

        _commandService.AddCommandHandler("night", (entity, args) =>
        {
            gameWorld.SetTime(0, 0);
        });

        _commandService.AddCommandHandler("createObjectFor", (entity, args) =>
        {
            _entityFactory.CreateObjectFor(entity, (ObjectModel)1337, entity.Transform.Position + new Vector3(3, 0, 0), entity.Transform.Rotation);
        });

        _commandService.AddCommandHandler("tp", (entity, args) =>
        {
            entity.Transform.Position = new Vector3(0, 0, 3);
        });

        _commandService.AddCommandHandler("i", (entity, args) =>
        {
            entity.Transform.Interior = args.ReadByte();
        });

        _commandService.AddCommandHandler("d", (entity, args) =>
        {
            entity.Transform.Dimension = args.ReadUShort();
        });

        _commandService.AddCommandHandler("runtimeobject", (entity, args) =>
        {
            var modelFactory = new ModelFactory();
            modelFactory.AddTriangle(new Vector3(2, 2, 0), new Vector3(0, 10, 0), new Vector3(10, 0, 0), "Metal1_128");
            modelFactory.AddTriangle(new Vector3(10, 0, 0), new Vector3(0, 10, 0), new Vector3(10, 10, 0), "Metal1_128");
            var dff = modelFactory.BuildDff();
            var col = modelFactory.BuildCol();
            assetsService.ReplaceModelFor(entity, dff, col, 1339);
            _entityFactory.CreateObject((ObjectModel)1339, entity.Transform.Position + new Vector3(15, 15, -5), Vector3.Zero);
        });


        _commandService.AddCommandHandler("restoreobject", (entity, args) =>
        {
            assetsService.RestoreModelFor(entity, 1339);
        });

        _commandService.AddCommandHandler("amiinwater", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _chatBox.OutputTo(entity, $"amiinwater: {playerElementComponent.IsInWater} {entity.Transform.Position}");
        });

        _commandService.AddCommandHandler("formatmoney", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _chatBox.OutputTo(entity, $"{123.123m.FormatMoney(new System.Globalization.CultureInfo("pl-PL"))}");
        });

        _commandService.AddAsyncCommandHandler("createObjectFor2", async (entity, args) =>
        {
            var pos = entity.Transform.Position + new Vector3(3, 0, 0);
            var obj = _entityFactory.CreateObjectFor(entity, (ObjectModel)1337, pos, entity.Transform.Rotation);
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(2000);
                obj.Position = pos + new Vector3(i, 0, 0);
                obj.Rotation = entity.Transform.Rotation;
            }
        });

        _commandService.AddCommandHandler("addtestmarker1", (entity, args) =>
        {
            spawnMarkersService.AddSpawnMarker(new PointSpawnMarker("test123", entity.Transform.Position));
            _chatBox.OutputTo(entity, "marker added1");
        });

        _commandService.AddCommandHandler("addtestmarker2", (entity, args) =>
        {
            spawnMarkersService.AddSpawnMarker(new DirectionalSpawnMarker("test direct", entity.Transform.Position, entity.Transform.Rotation.Z));
            _chatBox.OutputTo(entity, "marker added2");
        });

        _commandService.AddCommandHandler("testnotrace", (entity, args) =>
        {
            _logger.LogInformation("no trace");
        }, null);

        int counter = 0;
        _commandService.AddCommandHandler("counter", (entity, args) =>
        {
            counter++;
            _logger.LogInformation("Counter: {counter}", counter);
        });

        _commandService.AddCommandHandler("level", (entity, args) =>
        {
            var levelComponent = entity.GetRequiredComponent<LevelComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _chatBox.OutputTo(entity, $"Level: {levelComponent.Level}, exp: {levelComponent.Experience}");
        });

        _commandService.AddCommandHandler("setlevel", (entity, args) =>
        {
            uint level = args.ReadUInt();
            var levelComponent = entity.GetRequiredComponent<LevelComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            void handleLevelChanged(LevelComponent that, uint level, bool up)
            {
                _chatBox.OutputTo(entity, $"Level change: {level}, {up}");
            }
            levelComponent.LevelChanged += handleLevelChanged;
            levelComponent.Level = level;
            levelComponent.LevelChanged -= handleLevelChanged;
        });

        _commandService.AddCommandHandler("cefdevtools", (entity, args) =>
        {
            var adminComponent = entity.GetRequiredComponent<AdminComponent>();
            var blazorGuiComponent = entity.GetRequiredComponent<BrowserGuiComponent>();
            adminComponent.DevelopmentMode = true;
            blazorGuiComponent.DevTools = !blazorGuiComponent.DevTools;
            _chatBox.OutputTo(entity, $"Devtools {blazorGuiComponent.DevTools}");
        }, null);

        _commandService.AddCommandHandler("cefpath", (entity, args) =>
        {
            var blazorGuiComponent = entity.GetRequiredComponent<BrowserGuiComponent>();
            _chatBox.OutputTo(entity, $"Path {blazorGuiComponent.Path}");
        }, null);

        _commandService.AddAsyncCommandHandler("usernames", async (entity, args) =>
        {
            await using var userRepository = repositoryFactory.GetUserRepository();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var userNames = await userRepository.GetUserNamesByIds(new int[] { 1 });
            foreach (var item in userNames)
            {
                _chatBox.OutputTo(entity, $"{item.Key} = {item.Value}");
            }
        });

        _commandService.AddAsyncCommandHandler("setkind", async (entity, args) =>
        {
            await _vehiclesService.SetVehicleKind(entity.GetRequiredComponent<PlayerElementComponent>().OccupiedVehicle, 42);
        });

        _commandService.AddAsyncCommandHandler("kind", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var veh = playerElementComponent.OccupiedVehicle;
            var kind = veh.GetRequiredComponent<PrivateVehicleComponent>().Kind;
            _chatBox.OutputTo(entity, $"Kind: {kind}");
        });

        _commandService.AddAsyncCommandHandler("counterasync", async (entity, args) =>
        {
            counter++;
            _logger.LogInformation("Counter: {counter}", counter);
        });

        _commandService.AddAsyncCommandHandler("addvehevent", async (entity, args) =>
        {
            await _vehiclesService.AddVehicleEvent(entity.GetRequiredComponent<PlayerElementComponent>().OccupiedVehicle, 1);
        });

        _commandService.AddAsyncCommandHandler("vehevents", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var events = await _vehiclesService.GetAllVehicleEvents(playerElementComponent.OccupiedVehicle);
            _chatBox.OutputTo(entity, "Events:");
            foreach (var item in events)
            {
                _chatBox.OutputTo(entity, $"Event: {item.DateTime} - {item.EventType}");
            }
        });

        _commandService.AddCommandHandler("blazorguiopen", (entity, args) =>
        {
            entity.GetRequiredComponent<BrowserGuiComponent>().Path = "Counter";
        });

        _commandService.AddCommandHandler("blazorguiclose", (entity, args) =>
        {
            entity.GetRequiredComponent<BrowserGuiComponent>().Path = null;
        });

        _commandService.AddCommandHandler("kickme", (entity, args) =>
        {
            userManager.Kick(entity, "test");
        });

        _commandService.AddCommandHandler("getplayerbyname", (entity, args) =>
        {
            if (userManager.TryGetPlayerByName(args.ReadArgument(), out var foundPlayer))
            {
                _chatBox.OutputTo(entity, "found");
            }
            else
                _chatBox.OutputTo(entity, "not found");
        });

        _commandService.AddCommandHandler("findbyname", (entity, args) =>
        {
            var players = userManager.SearchPlayersByName(args.ReadArgument());
            _chatBox.OutputTo(entity, "found:");
            foreach (var item in players)
            {
                _chatBox.OutputTo(entity, $"Player: {item.GetRequiredComponent<UserComponent>().UserName}");
            }
        });

        _commandService.AddAsyncCommandHandler("addrating", async (entity, args) =>
        {
            var last = (await feedbackService.GetLastRating(entity, 1)) ?? (0, DateTime.MinValue);
            if (last.Item2.AddSeconds(3) > dateTimeProvider.Now)
            {
                _chatBox.OutputTo(entity, "możesz ocenić maksymalnie raz na 30sekund");
                return;
            }
            var rating = Random.Shared.Next(100);
            await feedbackService.ChangeLastRating(entity, 1, rating);
            _chatBox.OutputTo(entity, $"zmieniono ocenę z {rating} z {last.Item1}");
        });

        _commandService.AddAsyncCommandHandler("addopinion", async (entity, args) =>
        {
            await feedbackService.AddOpinion(entity, 1, string.Join(", ", args));
            _chatBox.OutputTo(entity, "Opinia dodana");
        });

        _commandService.AddCommandHandler("addprivatemarker", (entity, args) =>
        {
            void handleEntityEntered(MarkerElementComponent markerElementComponent, Entity enteredMarker, Entity enteredEntity)
            {
                entity.DestroyComponent(markerElementComponent);
            }

            PlayerPrivateElementComponent<MarkerElementComponent> marker = _entityFactory.CreateMarkerFor(entity, entity.Transform.Position with { X = entity.Transform.Position.X + 4 }, MarkerType.Checkpoint);
            marker.ElementComponent.EntityEntered = handleEntityEntered;
        });

        _commandService.AddAsyncCommandHandler("createmarkerforme", async (entity, args) =>
        {
            var marker = _entityFactory.CreateMarkerFor(entity, entity.Transform.Position, MarkerType.Cylinder);
            marker.ElementComponent.Size = 4;
            marker.ElementComponent.Color = Color.Red;
            while (true)
            {
                if (marker.ElementComponent.Size == 4)
                {
                    marker.ElementComponent.Size = 2;
                    marker.ElementComponent.Color = Color.Red;
                }
                else
                {
                    marker.ElementComponent.Size = 4;
                    marker.ElementComponent.Color = Color.Blue;
                }
                await Task.Delay(500);
            }
        });

        _commandService.AddAsyncCommandHandler("createmarkerforme2", async (entity, args) =>
        {
            var marker = _entityFactory.CreateMarkerFor(entity, new Vector3(-600.8877f, 240.88867f, 26.091864f), MarkerType.Cylinder);
            marker.ElementComponent.Size = 4;
            marker.ElementComponent.Color = Color.Red;
        });

        _commandService.AddCommandHandler("createmarkerforme3", (entity, args) =>
        {
            var marker = _entityFactory.CreateMarkerFor(entity, entity.Transform.Position, MarkerType.Cylinder);
            marker.ElementComponent.Size = 4;
            marker.ElementComponent.Color = Color.Red;
            marker.ElementComponent.EntityEntered = (markerElementComponent, enteredMarker, enteredEntity) =>
            {
                ;
            };
        });

        _commandService.AddCommandHandler("setinterior", (entity, args) =>
        {
            if (entity.Transform.Interior == 1)
                entity.Transform.Interior = 0;
            else
                entity.Transform.Interior = 1;
        });

        _commandService.AddCommandHandler("setinterior2", (entity, args) =>
        {
            entity.Transform.Interior = 1;
            var veh = _entityFactory.CreateVehicle(404, new Vector3(338.26562f, -87.75098f, 1.5197021f), Vector3.Zero);
            entity.GetRequiredComponent<PlayerElementComponent>().WarpIntoVehicle(veh);
        });

        _commandService.AddAsyncCommandHandler("setinterior2b", async (entity, args) =>
        {
            entity.Transform.InteriorChanged += (t, i) =>
            {
                chatBox.OutputTo(entity, $"Changed interior to: {i}");
            };
            entity.Transform.Interior = 1;
            var veh = _entityFactory.CreateVehicle(404, new Vector3(338.26562f, -87.75098f, 1.5197021f), Vector3.Zero);
            await Task.Delay(1000);
            entity.GetRequiredComponent<PlayerElementComponent>().WarpIntoVehicle(veh);
        });

        _commandService.AddCommandHandler("setinterior3", (entity, args) =>
        {
            entity.Transform.Interior = 1;
            entity.Transform.Interior = 0;
        });

        _commandService.AddCommandHandler("position", (entity, args) =>
        {
            _chatBox.OutputTo(entity, $"Position: {entity.Transform.Position}, rot: {entity.Transform.Rotation}, i: {entity.Transform.Interior}, d: {entity.Transform.Dimension}");
        });

        _commandService.AddCommandHandler("testargs", (entity, args) =>
        {
            var a = args.ReadInt();
            var b = args.ReadByte();
            var c = args.ReadInt();
            _chatBox.OutputTo(entity, $"Komenda wykonana, argumenty {a}, {b}, {c}");
        });

        _commandService.AddCommandHandler("testargs2", (entity, args) =>
        {
            var a = args.ReadPlayerEntity();
            var b = args.ReadPlayerEntity();
            _chatBox.OutputTo(entity, $"Komenda wykonana, argumenty {a} {b}");
        });

        _commandService.AddCommandHandler("admincmd", (entity, args) =>
        {
            _chatBox.OutputTo(entity, $"executed admin cmd");
        }, new string[] { "Admin" });
        
        _commandService.AddCommandHandler("enum", (entity, args) =>
        {
            _chatBox.OutputTo(entity, $"Enum value: {args.ReadEnum<TestEnum>()}");
        }, new string[] { "Admin" });
        
        _commandService.AddCommandHandler("cefblazor", (entity, args) =>
        {
            entity.GetRequiredComponent<BrowserGuiComponent>().LoadRemotePage("https://localhost:7149/");
            //entity.GetRequiredComponent<BrowserGuiComponent>().LoadRemotePage("http://localhost:5000/");
            _chatBox.OutputTo(entity, "Loaded");
        });

    }

    static int _hudPosition = 0;
}

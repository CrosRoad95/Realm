using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SlipeServer.Server.Services;
using System.Diagnostics;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;
using RealmCore.Console.Components.Vehicles;
using RealmCore.Server.Components.Peds;
using SlipeServer.Server.Enums;
using RealmCore.Server.Components.World;
using RealmCore.Server.Enums;
using RealmCore.Resources.Base;
using RealmCore.Server.Extensions.Resources;
using RealmCore.Server.Extensions;
using RealmCore.Console.Components.Huds;
using RealmCore.Resources.Nametags;
using RealmCore.Resources.ClientInterface;
using RealmCore.Module.Discord.Interfaces;
using RealmCore.Console.Components;
using RealmCore.Resources.ElementOutline;

namespace RealmCore.Console.Logic;

internal sealed class CommandsLogic
{
    private readonly RPGCommandService _commandService;
    private readonly IEntityFactory _entityFactory;
    private readonly RepositoryFactory _repositoryFactory;
    private readonly ItemsRegistry _itemsRegistry;
    private readonly IECS _ecs;
    private readonly IBanService _banService;
    private readonly IDiscordService _discordService;
    private readonly ChatBox _chatBox;
    private readonly ILogger<CommandsLogic> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IVehiclesService _vehiclesService;

    private class TestState
    {
        public string Text { get; set; }
    }

    public CommandsLogic(RPGCommandService commandService, IEntityFactory entityFactory, RepositoryFactory repositoryFactory,
        ItemsRegistry itemsRegistry, IECS ecs, IBanService banService, IDiscordService discordService, ChatBox chatBox, ILogger<CommandsLogic> logger,
        IDateTimeProvider dateTimeProvider, INametagsService nametagsService, IUsersService rpgUserManager, IVehiclesService vehiclesService,
        GameWorld gameWorld, IElementOutlineService elementOutlineService)
    {
        _commandService = commandService;
        _entityFactory = entityFactory;
        _repositoryFactory = repositoryFactory;
        _itemsRegistry = itemsRegistry;
        _ecs = ecs;
        _banService = banService;
        _discordService = discordService;
        _chatBox = chatBox;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _vehiclesService = vehiclesService;
        _commandService.AddCommandHandler("playtime", (entity, args) =>
        {
            if (entity.TryGetComponent(out PlayTimeComponent playTimeComponent))
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"playtime: {playTimeComponent.PlayTime}, total play time: {playTimeComponent.TotalPlayTime}");
            }
        });

        _commandService.AddCommandHandler("givemoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = decimal.Parse(args.First());
                moneyComponent.GiveMoney(amount);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"gave money: {amount}, total money: {moneyComponent.Money}");
            }
        });

        _commandService.AddCommandHandler("takemoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = decimal.Parse(args.First());
                moneyComponent.TakeMoney(amount);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"taken money: {amount}, total money: {moneyComponent.Money}");
            }
        });

        _commandService.AddCommandHandler("setmoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = decimal.Parse(args.First());
                moneyComponent.Money = amount;
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"taken money: {amount}, total money: {moneyComponent.Money}");
            }
        });

        _commandService.AddCommandHandler("money", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"total money: {moneyComponent.Money}");
            }
        });

        _commandService.AddCommandHandler("cv", (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent<VehicleUpgradesComponent>();
            vehicleEntity.AddComponent<MileageCounterComponent>();
            vehicleEntity.AddComponent(new VehicleFuelComponent(1, 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent<VehicleFocusableComponent>();
            vehicleEntity.AddComponent<VehiclePartDamageComponent>().AddPart(1, 1337);
        });

        _commandService.AddAsyncCommandHandler("cvprivate", async (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = await _entityFactory.CreateNewPrivateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent(new VehicleUpgradesComponent()).AddUpgrade(1);
            vehicleEntity.AddComponent<MileageCounterComponent>();
            vehicleEntity.AddComponent<VehicleEngineComponent>();
            vehicleEntity.AddComponent(new VehicleFuelComponent(1, 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent<VehiclePartDamageComponent>().AddPart(1, 1337);
            vehicleEntity.GetRequiredComponent<PrivateVehicleComponent>().AddAsOwner(entity);
        });

        _commandService.AddCommandHandler("exclusivecv", (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent<VehicleUpgradesComponent>();
            vehicleEntity.AddComponent<MileageCounterComponent>();
            vehicleEntity.AddComponent(new VehicleFuelComponent(1, 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent(new VehicleExclusiveAccessComponent(vehicleEntity));
        });

        _commandService.AddCommandHandler("noaccesscv", (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent<VehicleUpgradesComponent>();
            vehicleEntity.AddComponent<MileageCounterComponent>();
            vehicleEntity.AddComponent(new VehicleFuelComponent(1, 20, 20, 0.01, 2)).Active = true;
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
            veh.GetRequiredComponent<PrivateVehicleComponent>().AddAsOwner(entity);
        });

        _commandService.AddCommandHandler("accessinfo", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var veh = playerElementComponent.OccupiedVehicle;
            if (veh == null)
            {
                playerElementComponent.SendChatMessage("Enter vehicle!");
                return;
            }

            var privateVehicleComponent = veh.GetRequiredComponent<PrivateVehicleComponent>();
            playerElementComponent.SendChatMessage("Access info:");

            foreach (var vehicleAccess in privateVehicleComponent.PlayerAccesses)
            {
                playerElementComponent.SendChatMessage($"Access: ({vehicleAccess.UserId}) = Ownership={vehicleAccess.AccessType == 0}");
            }
        });

        _commandService.AddCommandHandler("testachievement", (entity, args) =>
        {
            var achievementsComponent = entity.GetRequiredComponent<AchievementsComponent>();
            achievementsComponent.UpdateProgress(1, 2, 10);
            entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"progressed achieviement 'test'");
        });

        _commandService.AddCommandHandler("addupgrade", (entity, args) =>
        {
            var jobUpgradesComponent = entity.GetRequiredComponent<JobUpgradesComponent>();
            try
            {
                if (jobUpgradesComponent.TryAddJobUpgrade(1, 1))
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage("Upgrade added");
                else
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage("Failed to add upgrade");
            }
            catch (Exception ex)
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Failed to add upgrade: {ex.Message}");
            }
        });

        _commandService.AddCommandHandler("addvehicleupgrade", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var veh = playerElementComponent.OccupiedVehicle;
            if (veh == null)
            {
                playerElementComponent.SendChatMessage("Enter vehicle!");
                return;
            }

            var vehicleUpgradeComponent = veh.GetRequiredComponent<VehicleUpgradesComponent>();
            if (vehicleUpgradeComponent.HasUpgrade(1))
            {
                playerElementComponent.SendChatMessage("You already have a upgrade!");
                return;
            }
            vehicleUpgradeComponent.AddUpgrade(1);
            playerElementComponent.SendChatMessage("Upgrade added");
        });

        _commandService.AddCommandHandler("comps", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SendChatMessage("Components:");
            foreach (var component in entity.Components)
            {
                playerElementComponent.SendChatMessage($"> {component}");
            }
        });

        _commandService.AddCommandHandler("additem", (entity, args) =>
        {
            if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                inventoryComponent.AddItem(_itemsRegistry, 1);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Test item added");
            }
        });
        _commandService.AddCommandHandler("additem2", (entity, args) =>
        {
            if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                inventoryComponent.AddItem(_itemsRegistry, 2);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Test item added");
            }
        });
        _commandService.AddAsyncCommandHandler("addtestdata", async (entity, args) =>
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
                achievementsComponent.UpdateProgress(1, 2, 10);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Updated achievement 'test' progress to 2");
            }

            {
                using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
                var vehicleEntity = await _entityFactory.CreateNewPrivateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
                vehicleEntity.AddComponent(new VehicleUpgradesComponent()).AddUpgrade(1);
                vehicleEntity.AddComponent(new MileageCounterComponent());
                vehicleEntity.AddComponent(new VehicleFuelComponent(1, 20, 20, 0.01, 2)).Active = true;
                vehicleEntity.AddComponent<VehiclePartDamageComponent>().AddPart(1, 1337);
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
        });

        _commandService.AddCommandHandler("cvforsale", (entity, args) =>
        {
            _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), Vector3.Zero, entityBuilder: entity =>
            {
                entity.AddComponent(new VehicleForSaleComponent(200));
            });
        });

        _commandService.AddCommandHandler("spawnbox", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            objectEntity.AddComponent(new LiftableWorldObjectComponent());
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
            using var e = _ecs.CreateEntity(Guid.NewGuid().ToString(), EntityTag.Unknown);
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
            var e = _ecs.CreateEntity(Guid.NewGuid().ToString(), EntityTag.Unknown);
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
            objectEntity.AddComponent<DurationBasedHoldInteractionComponent>();
        });

        _commandService.AddCommandHandler("spawnbox3", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            objectEntity.AddComponent<DurationBasedHoldInteractionWithRingEffectComponent>();
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
        });
        _commandService.AddAsyncCommandHandler("animationasync", async (entity, args) =>
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

        _commandService.AddAsyncCommandHandler("complexanimation", async (entity, args) =>
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

        _commandService.AddCommandHandler("mygroups", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SendChatMessage("Groups:");
            foreach (var item in entity.Components.OfType<GroupMemberComponent>())
            {
                playerElementComponent.SendChatMessage($"Group id: {item.GroupId}, rank: {item.Rank}, rank name: '{item.RankName}'");
            }
        });

        _commandService.AddCommandHandler("myfractions", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SendChatMessage("Fractions:");
            foreach (var item in entity.Components.OfType<FractionMemberComponent>())
            {
                playerElementComponent.SendChatMessage($"Fraction id: {item.FractionId}, rank: {item.Rank}, rank name: '{item.RankName}'");
            }
        });

        _commandService.AddCommandHandler("toggleadmindebug", (entity, args) =>
        {
            var adminComponent = entity.GetRequiredComponent<AdminComponent>();
            adminComponent.AdminTools = !adminComponent.AdminTools;
            adminComponent.InteractionDebugRenderingEnabled = !adminComponent.InteractionDebugRenderingEnabled;
        });

        _commandService.AddCommandHandler("createhud", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            entity.AddComponent(new SampleHud());
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
                playerElementComponent.SendChatMessage("Twoje konto jest już połączone z discordem.");
            }
            entity.TryDestroyComponent<PendingDiscordIntegrationComponent>();
            var pendingDiscordIntegrationComponent = new PendingDiscordIntegrationComponent();
            var code = pendingDiscordIntegrationComponent.GenerateAndGetDiscordConnectionCode();
            entity.AddComponent(pendingDiscordIntegrationComponent);
            playerElementComponent.SendChatMessage($"Aby połączyć konto wpisz na kanale discord #polacz-konto komendę: /polaczkonto {code}");
        });

        _commandService.AddCommandHandler("adduserupgrade", (entity, args) =>
        {
            var userComponent = entity.GetRequiredComponent<UserComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var i = Random.Shared.Next(0, 10);
            if (userComponent.TryAddUpgrade(i))
                playerElementComponent.SendChatMessage($"Pomyślnie dodano ulepszenie id {i}");
            else
                playerElementComponent.SendChatMessage($"Pomyślnie dodano ulepszenie id {i}");
        });

        _commandService.AddAsyncCommandHandler("ban", async (entity, args) =>
        {
            var userComponent = entity.GetRequiredComponent<UserComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            await _banService.BanUserId(userComponent.Id);
            playerElementComponent.Kick("test 123");
        });

        _commandService.AddCommandHandler("destroyattachedentity", (entity, args) =>
        {
            var userComponent = entity.GetRequiredComponent<UserComponent>();
            var attachedEntity = entity.GetRequiredComponent<AttachedEntityComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            attachedEntity.AttachedEntity.Dispose();
            if (entity.HasComponent<AttachedEntityComponent>())
            {
                playerElementComponent.SendChatMessage("Nie udalo sie zniszczyc");
            }
            else
            {
                playerElementComponent.SendChatMessage("Zniszczone");
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

        _commandService.AddAsyncCommandHandler("discordsendmessage", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var messageId = await _discordService.SendMessage(1079342213097607399, args.First());
            playerElementComponent.SendChatMessage($"Wysłano wiadomość, id: {messageId}");
        });

        _commandService.AddAsyncCommandHandler("discordsendmessagetouser", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var messageId = await _discordService.SendMessageToUser(659910279353729086, args.First());
            playerElementComponent.SendChatMessage($"Wysłano wiadomość, id: {messageId}");
        });

        Stream generateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        _commandService.AddAsyncCommandHandler("discordsendfile", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var messageId = await _discordService.SendFile(997787973775011853, generateStreamFromString("dowody"), "dowody_na_borsuka.txt", "potwierdzam");
            playerElementComponent.SendChatMessage($"Wysłano plik, id: {messageId}");
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
            nametagsService.SetNametagRenderingEnabled(entity, args.FirstOrDefault() == "true");
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
            nametagsService.SetLocalPlayerRenderingEnabled(entity, args.FirstOrDefault() == "true");
        });

        _commandService.AddCommandHandler("outlinerendering", (entity, args) =>
        {
            elementOutlineService.SetRenderingEnabled(entity, args.FirstOrDefault() == "true");
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
            entity.GetRequiredComponent<UserComponent>().SetSetting(1, args[0]);
        });

        _commandService.AddCommandHandler("removesetting", (entity, args) =>
        {
            entity.GetRequiredComponent<UserComponent>().RemoveSetting(1);
        });

        _commandService.AddCommandHandler("getsetting", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();

            var settingValue = entity.GetRequiredComponent<UserComponent>().GetSetting(1);

            playerElementComponent.SendChatMessage($"Setting1: {settingValue}");
        });


        _commandService.AddAsyncCommandHandler("whitelistmyserial", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var userComponent = entity.GetRequiredComponent<UserComponent>();

            if (await rpgUserManager.TryAddWhitelistedSerial(userComponent.Id, playerElementComponent.Client.Serial))
            {
                playerElementComponent.SendChatMessage($"Dodano serial");
            }
            else
            {
                playerElementComponent.SendChatMessage($"Nie udało się dodać");
            }
        });

        _commandService.AddAsyncCommandHandler("removewhitelistmyserial", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var userComponent = entity.GetRequiredComponent<UserComponent>();

            if (await rpgUserManager.TryRemoveWhitelistedSerial(userComponent.Id, playerElementComponent.Client.Serial))
            {
                playerElementComponent.SendChatMessage($"Usunięto serial");
            }
            else
            {
                playerElementComponent.SendChatMessage($"Nie udało się usunąć");
            }
        });

        _commandService.AddCommandHandler("addvisualupgrade", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            if (playerElementComponent.OccupiedVehicle.GetRequiredComponent<VehicleUpgradesComponent>().AddUniqueUpgrade(3))
            {
                playerElementComponent.SendChatMessage($"dodano wizualne ulepszenie");
            }
            else
            {
                playerElementComponent.SendChatMessage($"Nie udało się dodać wizualnego ulepszenia");
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
            await _vehiclesService.Despawn(playerElementComponent.OccupiedVehicle);
        });

        _commandService.AddAsyncCommandHandler("spawnback", async (entity, args) =>
        {
            var en = await _vehiclesService.SpawnById(int.Parse(args[0]));
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            if (en != null)
            {
                playerElementComponent.SendChatMessage("Spawned");
            }
            else
                playerElementComponent.SendChatMessage("Error while spawning");
        });

        _commandService.AddCommandHandler("inventoryoccupied", (entity, args) =>
        {
            var inv = entity.GetRequiredComponent<InventoryComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SendChatMessage($"Inventory: {inv.Number}/{inv.Size}");
        });
        
        _commandService.AddCommandHandler("giveitem4", (entity, args) =>
        {
            var inv = entity.GetRequiredComponent<InventoryComponent>();
            inv.AddSingleItem(_itemsRegistry, 4);
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SendChatMessage("Item given");
        });
        
        _commandService.AddCommandHandler("itemwithmetadata", (entity, args) =>
        {
            var inv = entity.GetRequiredComponent<InventoryComponent>();
            var item = inv.AddSingleItem(_itemsRegistry, 4, new Dictionary<string, object>
            {
                ["number"] = 1m
            });

            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SendChatMessage($"Item regular: {item.GetMetadata("number").GetType()}");
            playerElementComponent.SendChatMessage($"Item cast<int>: {item.GetMetadata<int>("number").GetType()}");
        });

        _discordService.AddTextBasedCommandHandler(1069962155539042314, "test", (userId, parameters) =>
        {
            _chatBox.Output($"Użytkownik o id {userId} wpisał komendę 'test' z parametrami: {parameters}");
            return Task.CompletedTask;
        });

        _discordService.AddTextBasedCommandHandler(997787973775011853, "gracze", async (userId, parameters) =>
        {
            var playerEntities = _ecs.PlayerEntities;
            await _discordService.SendMessage(997787973775011853, $"Gracze na serwerze: {string.Join(", ", playerEntities.Select(x => x.GetRequiredComponent<PlayerElementComponent>().Name))}");
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
            entity.Transform.Interior = byte.Parse(args.First());
        });

        _commandService.AddCommandHandler("d", (entity, args) =>
        {
            entity.Transform.Dimension = ushort.Parse(args.First());
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

        FontCollection collection = new();
        FontFamily family = collection.Add("Server/Fonts/Ratual.otf");
        Font font = family.CreateFont(24, FontStyle.Regular);

        _discordService.AddTextBasedCommandHandler(997787973775011853, "graczegrafika", async (userId, parameters) =>
        {
            var playerEntities = _ecs.PlayerEntities;
            int width = 480;
            int height = 60;

            DrawingOptions options = new()
            {
                GraphicsOptions = new()
                {
                    ColorBlendingMode = PixelColorBlendingMode.Multiply
                }
            };

            var encoder = new JpegEncoder()
            {
                Quality = 40
            };

            using var memoryStream = new MemoryStream();
            using var image = new Image<Rgba32>(width, height);

            image.Mutate(x => x.DrawText($"Gracze na serwerze: {string.Join(", ", playerEntities.Select(x => x.GetRequiredComponent<PlayerElementComponent>().Name))}", font, IronSoftware.Drawing.Color.White, new PointF(10, 10)));
            image.Save(memoryStream, encoder);
            memoryStream.Position = 0;
            await _discordService.SendFile(997787973775011853, memoryStream, "obrazek.jpg", "");
        });

        _discordService.AddTextBasedCommandHandler(997787973775011853, "testgrafika", async (userId, parameters) =>
        {
            int width = 480;
            int height = 60;

            DrawingOptions options = new()
            {
                GraphicsOptions = new()
                {
                    ColorBlendingMode = PixelColorBlendingMode.Multiply
                }
            };

            var encoder = new JpegEncoder()
            {
                Quality = 40
            };

            using var memoryStream = new MemoryStream();
            using Image<Rgba32> image = new(width, height);

            image.Mutate(x => x.DrawText(parameters, font, IronSoftware.Drawing.Color.White, new PointF(10, 10)));
            image.Save(memoryStream, encoder);
            memoryStream.Position = 0;
            await _discordService.SendFile(997787973775011853, memoryStream, "obrazek.jpg", "");
        });
    }

    static int _hudPosition = 0;
}

using Google.Protobuf.WellKnownTypes;
using Realm.Console.Components.Huds;
using Realm.Domain.Interfaces;
using Realm.Domain.Inventory;
using Realm.Module.Discord.Interfaces;
using Realm.Persistance.Data;
using Realm.Resources.Assets;
using Realm.Resources.Assets.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SlipeServer.Server.Services;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using static Realm.Domain.Components.Elements.PlayerElementComponent;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;

namespace Realm.Console.Logic;

internal sealed class CommandsLogic
{
    private readonly RPGCommandService _commandService;
    private readonly IEntityFactory _entityFactory;
    private readonly RepositoryFactory _repositoryFactory;
    private readonly ItemsRegistry _itemsRegistry;
    private readonly ECS _ecs;
    private readonly IBanService _banService;
    private readonly IDiscordService _discordService;
    private readonly ChatBox _chatBox;
    private readonly ILogger<CommandsLogic> _logger;

    private class TestState
    {
        public string Text { get; set; }
    }

    public CommandsLogic(RPGCommandService commandService, IEntityFactory entityFactory, RepositoryFactory repositoryFactory,
        ItemsRegistry itemsRegistry, ECS ecs, IBanService banService, IDiscordService discordService, ChatBox chatBox, ILogger<CommandsLogic> logger)
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
            vehicleEntity.AddComponent<VehiclePartDamageComponent>().AddPart(1, 1337);
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("cvprivate", async (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = await _entityFactory.CreateNewPrivateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent(new VehicleUpgradesComponent()).AddUpgrade(1);
            vehicleEntity.AddComponent(new MileageCounterComponent());
            vehicleEntity.AddComponent(new VehicleFuelComponent("default", 20, 20, 0.01, 2)).Active = true;
            vehicleEntity.AddComponent<VehiclePartDamageComponent>().AddPart(1, 1337);
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

        _commandService.AddCommandHandler("spawnboard", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject((SlipeServer.Server.Enums.ObjectModel)3077, entity.Transform.Position + new Vector3(4, 0, -1), Vector3.Zero);
            objectEntity.AddComponent(new LiftableWorldObjectComponent());
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("spawnboxmany", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(SlipeServer.Server.Enums.ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.6f), Vector3.Zero);
            objectEntity.AddComponent(new LiftableWorldObjectComponent());
            var objectEntity1 = _entityFactory.CreateObject(SlipeServer.Server.Enums.ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4 + 0.8f, 0, -0.6f), Vector3.Zero);
            objectEntity1.AddComponent(new LiftableWorldObjectComponent());
            var objectEntity2 = _entityFactory.CreateObject(SlipeServer.Server.Enums.ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0 + 0.8f, -0.6f), Vector3.Zero);
            objectEntity2.AddComponent(new LiftableWorldObjectComponent());
            var objectEntity3 = _entityFactory.CreateObject(SlipeServer.Server.Enums.ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4 + 0.8f, 0 + 0.8f, -0.6f), Vector3.Zero);
            objectEntity3.AddComponent(new LiftableWorldObjectComponent());
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("hud3d", async (entity, args) =>
        {
            var e = _ecs.CreateEntity(Guid.NewGuid().ToString(), Entity.EntityTag.Unknown);
            e.Transform.Position = entity.Transform.Position + new Vector3(-4, 0, 0);
            e.AddComponent(new Hud3dComponent<TestState>(e => e
                .AddRectangle(Vector2.Zero, new Size(100, 100), Color.Red)
                .AddRectangle(new Vector2(25, 25), new Size(50, 50), Color.Green)
                .AddText(x => x.Text, new Vector2(0,0), new Size(100, 100), Color.Blue)
                , new TestState
                {
                    Text = "test 1",
                }));

            await Task.Delay(2000);
            _ecs.Destroy(e);
        });
        
        _commandService.AddCommandHandler("hud3d2", async (entity, args) =>
        {
            var e = _ecs.CreateEntity(Guid.NewGuid().ToString(), Entity.EntityTag.Unknown);
            e.Transform.Position = entity.Transform.Position + new Vector3(-4, 0, 0);
            var hud3d = e.AddComponent(new Hud3dComponent<TestState>(e => e
                .AddRectangle(Vector2.Zero, new Size(200, 200), Color.Red)
                .AddRectangle(new Vector2(25, 25), new Size(50, 50), Color.Green)
                .AddText(x => x.Text, new Vector2(0,0), new Size(200, 200), Color.White)
                , new TestState
                {
                    Text = "test 1",
                }));

            int i = 0;
            while(true)
            {
                await Task.Delay(1000 / 60);
                hud3d.UpdateState(x => x.Text = $"time {DateTime.Now} {i++}");
            }
        });

        _commandService.AddCommandHandler("spawnbox2", (entity, args) =>
        {
            var objectEntity = _entityFactory.CreateObject(SlipeServer.Server.Enums.ObjectModel.Gunbox, entity.Transform.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
            objectEntity.AddComponent<DurationBasedHoldInteractionComponent>();
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("animation", (entity, args) =>
        {
            if (System.Enum.TryParse<Animation>(args.FirstOrDefault(), out var animation))
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
            if (System.Enum.TryParse<Animation>(args.FirstOrDefault(), out var animation))
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
            if (System.Enum.TryParse<Animation>(args.FirstOrDefault(), out var animation))
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
            entity.AddComponent(new SampleHud());
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("movehud", (entity, args) =>
        {
            var sampleHud = entity.GetRequiredComponent<SampleHud>();
            sampleHud.Position = new Vector2(0, hudPosition++ * 10);
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("createstatefulhud", (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var sampleHud = entity.AddComponent(new SampleStatefulHud(new SampleHudState
            {
                Text1 = "text1",
                Text2 = "text2",
            }));
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("updatestate", (entity, args) =>
        {
            var sampleHud = entity.GetRequiredComponent<SampleStatefulHud>();
            sampleHud.Update();
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("destroyhuds", (entity, args) =>
        {
            entity.TryDestroyComponent<SampleHud>();
            entity.TryDestroyComponent<SampleStatefulHud>();
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
        
        _commandService.AddCommandHandler("ban", async (entity, args) =>
        {
            var accountComponent = entity.GetRequiredComponent<AccountComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            await _banService.BanUserId(accountComponent.Id);
            playerElementComponent.Kick("test 123");
        });
        
        _commandService.AddCommandHandler("destroyattachedentity", (entity, args) =>
        {
            var accountComponent = entity.GetRequiredComponent<AccountComponent>();
            var attachedEntity = entity.GetRequiredComponent<AttachedEntityComponent>();
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            _ecs.Destroy(attachedEntity.AttachedEntity);
            if (entity.HasComponent<AttachedEntityComponent>())
            {
                playerElementComponent.SendChatMessage("Nie udalo sie zniszczyc");
            }
            else
            {
                playerElementComponent.SendChatMessage("Zniszczone");
            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("discordsendmessage", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var messageId = await _discordService.SendMessage(1079342213097607399, args.First());
            playerElementComponent.SendChatMessage($"Wysłano wiadomość, id: {messageId}");
        });

        _commandService.AddCommandHandler("discordsendmessagetouser", async (entity, args) =>
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

        _commandService.AddCommandHandler("discordsendfile", async (entity, args) =>
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
            return Task.CompletedTask;
        });

        _discordService.AddTextBasedCommandHandler(1069962155539042314, "test", (userId, parameters) =>
        {
            _chatBox.Output($"Użytkownik o id {userId} wpisał komendę 'test' z parametrami: {parameters}");
            return Task.CompletedTask;
        });

        _discordService.AddTextBasedCommandHandler(997787973775011853, "gracze", async (userId, parameters) =>
        {
            var playerEntities = _ecs.GetPlayerEntities();
            await _discordService.SendMessage(997787973775011853, $"Gracze na serwerze: {string.Join(", ",playerEntities.Select(x => x.GetRequiredComponent<PlayerElementComponent>().Name))}");
        });


        FontCollection collection = new();
        FontFamily family = collection.Add("Server/Fonts/Ratual.otf");
        Font font = family.CreateFont(24, FontStyle.Regular);

        _discordService.AddTextBasedCommandHandler(997787973775011853, "graczegrafika", async (userId, parameters) =>
        {
            var playerEntities = _ecs.GetPlayerEntities();
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

            using (var memoryStream = new MemoryStream())
            using (Image<Rgba32> image = new(width, height))
            {
                image.Mutate(x => x.DrawText($"Gracze na serwerze: {string.Join(", ", playerEntities.Select(x => x.GetRequiredComponent<PlayerElementComponent>().Name))}", font, IronSoftware.Drawing.Color.White, new SixLabors.ImageSharp.PointF(10, 10)));
                image.Save(memoryStream, encoder);
                memoryStream.Position = 0;
                await _discordService.SendFile(997787973775011853, memoryStream, "obrazek.jpg", "");
            }
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

            using (var memoryStream = new MemoryStream())
            using (Image<Rgba32> image = new(width, height))
            {
                image.Mutate(x => x.DrawText(parameters, font, IronSoftware.Drawing.Color.White, new SixLabors.ImageSharp.PointF(10, 10)));
                image.Save(memoryStream, encoder);
                memoryStream.Position = 0;
                await _discordService.SendFile(997787973775011853, memoryStream, "obrazek.jpg", "");
            }
        });
    }

    static int hudPosition = 0;
}

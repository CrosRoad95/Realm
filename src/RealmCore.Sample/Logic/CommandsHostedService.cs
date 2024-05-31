using Polly.RateLimit;
using RealmCore.Resources.MapNames;
using RealmCore.Sample.HudLayers;
using RealmCore.Server.Modules.Elements.Focusable;
using RealmCore.Server.Modules.Players.Money;
using RealmCore.Server.Modules.Search;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Color = System.Drawing.Color;

namespace RealmCore.Sample.Logic;

internal sealed class CommandsHostedService : IHostedService
{
    private readonly RealmCommandService _commandService;
    private readonly IElementFactory _elementFactory;
    private readonly ItemsCollection _itemsCollection;
    private readonly ChatBox _chatBox;
    private readonly ILogger<CommandsHostedService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUsersService _usersService;
    private readonly IVehiclesService _vehiclesService;
    private readonly IElementOutlineService _elementOutlineService;
    private readonly IMoneyHistoryService _userMoneyHistoryService;
    private readonly IMapNamesService _mapNamesService;
    private readonly IVehiclesInUse _vehiclesInUse;
    private readonly IServiceProvider _serviceProvider;
    private readonly IElementCollection _elementCollection;
    private readonly RealmDiscordService? _discordService;

    private class TestState
    {
        public string Text { get; set; }
    }

    private enum TestEnum
    {
        Test1,
        Test2,
    }

    public CommandsHostedService(RealmCommandService commandService, IElementFactory elementFactory,
        ItemsCollection itemsCollection, ChatBox chatBox, ILogger<CommandsHostedService> logger,
        IDateTimeProvider dateTimeProvider, INametagsService nametagsService, IUsersService usersService, IVehiclesService vehiclesService,
        GameWorld gameWorld, IElementOutlineService elementOutlineService, IAssetsService assetsService, ISpawnMarkersService spawnMarkersService, IOverlayService overlayService, AssetsCollection assetsCollection, VehicleUpgradesCollection vehicleUpgradeCollection, VehicleEnginesCollection vehicleEnginesCollection, IMoneyHistoryService userMoneyHistoryService, IMapNamesService mapNamesService, IVehiclesInUse vehiclesInUse, IServiceProvider serviceProvider, IElementCollection elementCollection, RealmDiscordService? discordService = null)
    {
        _commandService = commandService;
        _elementFactory = elementFactory;
        _itemsCollection = itemsCollection;
        _chatBox = chatBox;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _usersService = usersService;
        _vehiclesService = vehiclesService;
        _elementOutlineService = elementOutlineService;
        _userMoneyHistoryService = userMoneyHistoryService;
        _mapNamesService = mapNamesService;
        _vehiclesInUse = vehiclesInUse;
        _serviceProvider = serviceProvider;
        _elementCollection = elementCollection;
        _discordService = discordService;
        var debounce = new Debounce(500);
        var debounceCounter = 0;

        _commandService.AddAsyncCommandHandler("cmdbasic", async ([CallingPlayer] RealmPlayer player, [Range(1, 20)] int a, int b) =>
        {
            await Task.Delay(100);
            _chatBox.Output($"foo: {a}, {b}");
        });
        
        _commandService.AddAsyncCommandHandler("cmdplr", async (RealmPlayer player, CancellationToken cancellationToken, int x) =>
        {
            _chatBox.Output($"foo: {player.Name} {cancellationToken} x={x}");
        });

        _commandService.AddAsyncCommandHandler("debounce", async (player, args, token) =>
        {
            debounceCounter++;
            await debounce.InvokeAsync(() =>
            {
                _chatBox.OutputTo(player, $"Counter={debounceCounter}, {DateTime.Now}");
            }, token);
        });

        _commandService.AddAsyncCommandHandler("fadecamera", async (player, args, token) =>
        {
            await player.FadeCameraAsync(CameraFade.Out, 5, token);
            await player.FadeCameraAsync(CameraFade.In, 5, token);
        });

        _commandService.AddAsyncCommandHandler("fadecamera2", async (player, args, token) =>
        {
            var cancelationTokenSource = new CancellationTokenSource();
            cancelationTokenSource.CancelAfter(2000);
            await player.FadeCameraAsync(CameraFade.Out, 5, cancelationTokenSource.Token);
            await player.FadeCameraAsync(CameraFade.In, 5, token);
        });

        #region Commands for focusable tests
        _commandService.AddCommandHandler("focusable", (player, args) =>
        {
            var position = player.Position + player.Forward * new Vector3(4, 0, 0);
            var worldObject = _elementFactory.CreateFocusableObject(new Location(position, player.Rotation), ObjectModel.Gunbox);
            worldObject.PlayerFocused += (that, player) =>
            {
                var playerName = player.Name;
                _chatBox.Output($"Player {playerName} focused, focused elements {worldObject.FocusedPlayerCount}");
                _logger.LogInformation($"Player {playerName} focused, focused elements {worldObject.FocusedPlayerCount}");
            };
            worldObject.PlayerLostFocus += (that, player) =>
            {
                var playerName = player.Name;
                _chatBox.Output($"Player {playerName} lost focus, focused elements {worldObject.FocusedPlayerCount}");
                _logger.LogInformation($"Player {playerName} lost focus, focused elements {worldObject.FocusedPlayerCount}");
            };

            _chatBox.OutputTo(player, "Created focusable");
        });

        _commandService.AddAsyncCommandHandler("outline1", async (player, args, token) =>
        {
            var worldObject = _elementFactory.CreateObject(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), ObjectModel.Gunbox);
            _elementOutlineService.SetElementOutline(worldObject, Color.Red);
            _chatBox.OutputTo(player, "Created outline");
            await Task.Delay(2000, token);
            _elementOutlineService.RemoveElementOutline(worldObject);
            _chatBox.OutputTo(player, "Destroyed outline");
        });

        _commandService.AddAsyncCommandHandler("outline2", async (player, args, token) =>
        {
            var worldObject = _elementFactory.CreateObject(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), ObjectModel.Gunbox);
            _elementOutlineService.SetElementOutline(worldObject, Color.Red);
            _chatBox.OutputTo(player, "Created outline");
            await Task.Delay(2000, token);
            worldObject.Destroy();
            _chatBox.OutputTo(player, "Destroyed player");
        });
        #endregion

        _commandService.AddCommandHandler("playtime", (player, args) =>
        {
            _chatBox.OutputTo(player, $"playtime: {player.PlayTime.PlayTime}, total play time: {player.PlayTime.TotalPlayTime}");
        });

        _commandService.AddCommandHandler("givemoney", (player, args) =>
        {
            var amount = args.ReadDecimal();
            player.Money.Give(amount);
            _chatBox.OutputTo(player, $"give money: {amount}, total money: {player.Money.Amount}");
        });

        _commandService.AddCommandHandler("takemoney", (player, args) =>
        {
            var amount = args.ReadDecimal();
            player.Money.Take(amount);
            _chatBox.OutputTo(player, $"take money: {amount}, total money: {player.Money.Amount}");
        });

        _commandService.AddCommandHandler("setmoney", (player, args) =>
        {
            var amount = args.ReadDecimal();
            player.Money.Amount = amount;
            _chatBox.OutputTo(player, $"set money: {amount}, total money: {player.Money.Amount}");
        });

        _commandService.AddCommandHandler("money", (player, args) =>
        {
            _chatBox.OutputTo(player, $"total money: {player.Money.Amount}");
        });

        _commandService.AddCommandHandler("cvwithlicenserequired", (player, args) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)args.ReadUShort());
            vehicle.AccessController = new VehicleLicenseRequirementAccessController(10);
        });

        _commandService.AddCommandHandler("givelicense10", (player, args) =>
        {
            if (player.Licenses.TryAdd(10))
                _chatBox.OutputTo(player, $"License 10 added");
        });

        _commandService.AddCommandHandler("cv", (player, args) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)args.ReadUShort());
            vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
            vehicle.PartDamage.AddPart(1, 1337);
            vehicle.AccessController = new VehicleExclusiveAccessController(player);
            _chatBox.OutputTo(player, $"veh created");
        });

        _commandService.AddAsyncCommandHandler("cvprivate", async (player, args, token) =>
        {
            var vehicle = await _vehiclesService.CreatePersistantVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)404, token);
            if (vehicle == null)
                return;

            vehicle.Upgrades.AddUpgrade(1);
            vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
            vehicle.PartDamage.AddPart(1, 1337);
            vehicle.Access.AddAsOwner(player);
            _chatBox.OutputTo(player, $"Stworzono pojazd o id: {vehicle.VehicleId}");
        });
        
        _commandService.AddCommandHandler("cvveh", (player, args) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)404);
            vehicle.Upgrades.AddUpgrade(1);
            vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
            vehicle.PartDamage.AddPart(1, 1337);
            vehicle.Access.AddAsOwner(player);
            _chatBox.OutputTo(player, "Stworzono pojazd.");
        });
        
        _commandService.AddCommandHandler("removeowner", (player, args) =>
        {
            player.Vehicle!.Access.TryRemoveAccess(player);
        });
        
        _commandService.AddAsyncCommandHandler("converttopersistent", async (player, args, token) =>
        {
            if(player.Vehicle == null)
            {
                _chatBox.OutputTo(player, "Wejdź do pojazdu.");
                return;
            }
            var vehicle = await _vehiclesService.ConvertToPersistantVehicle(player.Vehicle, token);
            if (vehicle == null)
                return;

            vehicle.Access.AddAsOwner(player);
            _chatBox.OutputTo(player, $"Skonwertowano pojazd, id: {vehicle.VehicleId}");
        });

        _commandService.AddAsyncCommandHandler("spawnveh", async (player, args, token) =>
        {
            var vehicleLoader = player.GetRequiredService<IVehicleLoader>();
            var vehicle = await vehicleLoader.LoadVehicleById(args.ReadInt(), token);
            var location = player.GetLocation(player.GetPointFromDistanceRotationOffset(3));
            vehicle.SetLocation(location);
            _chatBox.OutputTo(player, $"Załadowano pojazd na pozycji: {location}");
        });

        _commandService.AddAsyncCommandHandler("despawn", async (player, args, token) =>
        {
            if(player.Vehicle == null)
            {
                _chatBox.OutputTo(player, "Wejdź do pojazdu.");
                return;
            }
            await _vehiclesService.Destroy(player.Vehicle);
        });
        
        _commandService.AddCommandHandler("vehiclesinuse", (player, args) =>
        {
            _chatBox.OutputTo(player, $"Pojazdy w użyciu: {string.Join(", ", _vehiclesInUse.ActiveVehiclesIds)}");
        });
        
        _commandService.AddCommandHandler("gp", (player, args) =>
        {
            _chatBox.OutputTo(player, player.GetLocation().ToString());
        });
        
        _commandService.AddCommandHandler("myveh", (player, args) =>
        {
            _chatBox.OutputTo(player, player.Vehicle?.ToString() ?? "brak");
        });
        
        _commandService.AddCommandHandler("showaccess", (player, args) =>
        {
            _chatBox.OutputTo(player, player.Vehicle?.AccessController?.ToString() ?? "brak");
        });
        
        _commandService.AddCommandHandler("savemyveh", (player, args) =>
        {
            var vehicle = player.Vehicle ?? throw new InvalidOperationException();
            vehicle.GetRequiredService<IElementSaveService>().Save();
        });

        _commandService.AddCommandHandler("exclusivecv", (player, args) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)404);
            vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
            vehicle.AccessController = new VehicleExclusiveAccessController(player);
        });

        _commandService.AddCommandHandler("noaccesscv", (player, args) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)404);
            vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
            vehicle.AccessController = VehicleNoAccessController.Instance;
        });

        _commandService.AddAsyncCommandHandler("privateblip", async (player, args, token) =>
        {
            // TODO:
            //using var scopedelementFactory = _elementFactory.CreateScopedelementFactory(player);
            //var blipElementComponent = scopedelementFactory.CreateBlip(BlipIcon.Pizza, player.Position);
            //await Task.Delay(1000);
            //player.DestroyComponent(scopedelementFactory.LastCreatedComponent);
        });

        _commandService.AddCommandHandler("addmeasowner", (player, args) =>
        {
            var vehicle = player.Vehicle;
            if(vehicle != null)
                vehicle.Access.AddAsOwner(player);
        });

        _commandService.AddCommandHandler("accessinfo", (player, args) =>
        {
            if (player.Vehicle is not RealmVehicle vehicle)
            {
                _chatBox.OutputTo(player, "Enter vehicle!");
                return;
            }

            _chatBox.OutputTo(player, "Access info:");

            foreach (var vehicleAccess in vehicle.Access)
            {
                _chatBox.OutputTo(player, $"Access: ({vehicleAccess.UserId}) = Ownership={vehicleAccess.IsOwner}");
            }
        });

        _commandService.AddAsyncCommandHandler("accessinfobyid", async (player, args, token) =>
        {
            var vehicleRepository = player.GetRequiredService<IVehicleRepository>();
            var access = await vehicleRepository.GetAllVehicleAccesses(args.ReadInt(), token);
            if (access == null)
            {
                _chatBox.OutputTo(player, "Vehicle not found");
                return;
            }


            _chatBox.OutputTo(player, "Access info:");

            foreach (var vehicleAccess in access)
            {
                _chatBox.OutputTo(player, $"Access: ({vehicleAccess.UserId}) = Ownership={vehicleAccess.AccessType == 0}");
            }
        });

        _commandService.AddCommandHandler("testachievement", (player, args) =>
        {
            player.Achievements.UpdateProgress(1, 2, 10);
            _chatBox.OutputTo(player, $"progressed achieviement 'test'");
        });

        _commandService.AddCommandHandler("addupgrade", (player, args) =>
        {
            if (player.JobUpgrades.TryAdd(1, 1))
                _chatBox.OutputTo(player, "Upgrade added");
            else
                _chatBox.OutputTo(player, "Failed to add upgrade");
        });

        _commandService.AddCommandHandler("addvehicleupgrade", (player, args) =>
        {
            if (player.Vehicle is not RealmVehicle vehicle)
            {
                _chatBox.OutputTo(player, "Enter vehicle!");
                return;
            }

            if (vehicle.Upgrades.HasUpgrade(1))
            {
                _chatBox.OutputTo(player, "You already have a upgrade!");
                return;
            }
            vehicle.Upgrades.AddUpgrade(1);
            _chatBox.OutputTo(player, "Upgrade added");
        });

        _commandService.AddAsyncCommandHandler("addtestdata", async (player, args, token) =>
        {
            if (player.JobUpgrades.TryAdd(1, 1))
                _chatBox.OutputTo(player, "Upgrade added");
            else
                _chatBox.OutputTo(player, "Failed to add upgrade");

            if (player.Inventory.TryGetPrimary(out var inventory))
            {
                inventory.AddItem(_itemsCollection, 1);
                _chatBox.OutputTo(player, $"Test item added");
            }

            if (player.Licenses.TryAdd(1))
                _chatBox.OutputTo(player, $"Test license added: 'test123' of id 1");

            player.Money.Amount = (decimal)Random.Shared.NextDouble() * 1000;
            _chatBox.OutputTo(player, $"Set money to: {player.Money}");


            player.Achievements.UpdateProgress(1, 2, 10);
            _chatBox.OutputTo(player, $"Updated achievement 'test' progress to 2");

            {
                var vehicle = await _vehiclesService.CreatePersistantVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)404, token);
                if (vehicle == null)
                    return;

                vehicle.Upgrades.AddUpgrade(1);
                vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
                vehicle.PartDamage.AddPart(1, 1337);
            }
        });

        _commandService.AddCommandHandler("giveexperience", (player, args) =>
        {
            var amount = args.ReadUInt();
            player.Level.GiveExperience(amount);
            _chatBox.OutputTo(player, $"gave experience: {amount}, level: {player.Level.Current}, experience: {player.Level.Experience}");
        });

        _commandService.AddCommandHandler("cvforsale", (player, args) =>
        {
            _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), Vector3.Zero), (VehicleModel)404);
        });

        _commandService.AddAsyncCommandHandler("privateoutlinetest", async (player, args, token) =>
        {
            var @object = _elementFactory.CreateObject(new Location(player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero), ObjectModel.Gunbox);
            await Task.Delay(2000, token);
            elementOutlineService.SetElementOutlineForPlayer(player, @object, Color.Red);
            await Task.Delay(1000, token);
            elementOutlineService.RemoveElementOutlineForPlayer(player, @object);
            _chatBox.OutputTo(player, "removed");
        });

        _commandService.AddCommandHandler("spawnbox", (player, args) =>
        {
            var worldObject = _elementFactory.CreateFocusableObject(new Location(player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero), ObjectModel.Gunbox);
            worldObject.Interaction = new LiftableInteraction();
        });
        
        _commandService.AddCommandHandler("spawnscopedbox", (player, args) =>
        {
            var worldObject = player.ElementFactory.CreateFocusableObject(new Location(player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero), ObjectModel.Gunbox);
            worldObject.Interaction = new LiftableInteraction();
        });

        _commandService.AddCommandHandler("spawnboxfront", (player, args) =>
        {
            var front = player.GetPointFromDistanceRotation(2);
            var worldObject = _elementFactory.CreateObject(new Location(front), ObjectModel.Gunbox);
            worldObject.LookAt(player);
        });

        _commandService.AddCommandHandler("spawnmybox", (player, args) =>
        {
            var worldObject = _elementFactory.CreateObject(new Location(player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero), ObjectModel.Gunbox);
            worldObject.Interaction = new LiftableInteraction();
            worldObject.TrySetOwner(player);
        });

        _commandService.AddCommandHandler("setsetting", (player, args) =>
        {
            player.Settings.Set(1, args.ReadArgument());
            _chatBox.OutputTo(player, "set");
        });

        _commandService.AddCommandHandler("removesetting", (player, args) =>
        {
            player.Settings.TryRemove(1);
            _chatBox.OutputTo(player, "remove");
        });

        _commandService.AddCommandHandler("getsetting", (player, args) =>
        {
            player.Settings.TryGet(1, out var value);
            _chatBox.OutputTo(player, $"Setting1: {value}");
        });

        _commandService.AddCommandHandler("spawncolshapeforme", (player, args) =>
        {
            var marker = player.ElementFactory.CreateMarker(new Location(player.Position + new Vector3(4, 0, 0), Vector3.Zero), MarkerType.Arrow, 2, Color.Red);
            marker.CollisionShape.ElementEntered += (that, enteredElement) =>
            {
                _chatBox.Output($"entered2 {enteredElement}");
            };
        });
        // TODO:
        //_commandService.AddCommandHandler("spawnboxforme", (player, args) =>
        //{
        //using var scopedelementFactory = _elementFactory.CreateScopedelementFactory(player);
        //    var objectEntity = scopedelementFactory.CreateObject(ObjectModel.Gunbox, player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
        //    objectEntity.AddComponent<LiftableWorldObjectComponent>();
        //});

        //_commandService.AddAsyncCommandHandler("spawntempbox", async (player, args) =>
        //{
        //    var objectEntity = _elementFactory.CreateObject(ObjectModel.Gunbox, player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
        //    objectEntity.AddComponent(new LiftableWorldObjectComponent());
        //    await Task.Delay(5000);
        //    objectEntity.Dispose();
        //});

        //_commandService.AddCommandHandler("spawnboard", (player, args) =>
        //{
        //    var objectEntity = _elementFactory.CreateObject((ObjectModel)3077, player.Position + new Vector3(4, 0, -1), Vector3.Zero);
        //    objectEntity.AddComponent(new LiftableWorldObjectComponent());
        //});

        //_commandService.AddCommandHandler("spawnboxmany", (player, args) =>
        //{
        //    var objectEntity = _elementFactory.CreateObject(ObjectModel.Gunbox, player.Position + new Vector3(4, 0, -0.6f), Vector3.Zero);
        //    objectEntity.AddComponent(new LiftableWorldObjectComponent());
        //    var objectEntity1 = _elementFactory.CreateObject(ObjectModel.Gunbox, player.Position + new Vector3(4 + 0.8f, 0, -0.6f), Vector3.Zero);
        //    objectEntity1.AddComponent(new LiftableWorldObjectComponent());
        //    var objectEntity2 = _elementFactory.CreateObject(ObjectModel.Gunbox, player.Position + new Vector3(4, 0 + 0.8f, -0.6f), Vector3.Zero);
        //    objectEntity2.AddComponent(new LiftableWorldObjectComponent());
        //    var objectEntity3 = _elementFactory.CreateObject(ObjectModel.Gunbox, player.Position + new Vector3(4 + 0.8f, 0 + 0.8f, -0.6f), Vector3.Zero);
        //    objectEntity3.AddComponent(new LiftableWorldObjectComponent());
        //});

        //_commandService.AddAsyncCommandHandler("hud3d", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //using var e = _elementFactory.CreateEntity();
        //    e.AddComponent(new Hud3dComponent<TestState>(e => e
        //        .AddRectangle(Vector2.Zero, new Size(100, 100), Color.Red)
        //        .AddRectangle(new Vector2(25, 25), new Size(50, 50), Color.Green)
        //        .AddText(x => x.Text, new Vector2(0, 0), new Size(100, 100), Color.Blue),
        //        player.Position + new Vector3(-4, 0, 0),
        //        new TestState
        //        {
        //            Text = "test 1",
        //        }));

        //    await Task.Delay(2000);
        //});

        //_commandService.AddAsyncCommandHandler("hud3d2", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var e = _elementFactory.CreateEntity();
        //    var hud3d = e.AddComponent(new Hud3dComponent<TestState>(e => e
        //        .AddRectangle(Vector2.Zero, new Size(200, 200), Color.Red)
        //        .AddRectangle(new Vector2(25, 25), new Size(50, 50), Color.Green)
        //        .AddText(x => x.Text, new Vector2(0, 0), new Size(200, 200), Color.White),
        //        player.Position + new Vector3(-4, 0, 0),
        //        new TestState
        //        {
        //            Text = "test 1",
        //        }));

        //    int i = 0;
        //    while (true)
        //    {
        //        await Task.Delay(1000 / 60);
        //        hud3d.UpdateState(x => x.Text = $"time {_dateTimeProvider.Now} {i++}");
        //    }
        //});

        //_commandService.AddCommandHandler("spawnbox2", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var objectEntity = _elementFactory.CreateObject(ObjectModel.Gunbox, player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
        //    objectEntity.AddComponent(new DurationBasedHoldInteractionWithRingEffectComponent(overlayService));
        //});

        //_commandService.AddCommandHandler("stopanimation", (player, args) =>
        //{
        //    player.GetRequiredComponent<PlayerElementComponent>().StopAnimation();
        //});

        _commandService.AddAsyncCommandHandler("animation", async (player, args, token) =>
        {
            var animationName = args.ReadArgument();
            if (Enum.TryParse<Animation>(animationName, out var animation))
            {
                try
                {
                    await player.DoAnimationAsync(animation, cancellationToken: token);
                }
                catch (NotSupportedException)
                {
                    _chatBox.OutputTo(player, $"Animation '{animationName}' is not supported");
                }
            }
            else
                _chatBox.OutputTo(player, $"Animation '{animationName}' not found.");
        });

        //_commandService.AddAsyncCommandHandler("animationasync", async (player, args) =>
        //{
        //    var animationName = args.ReadArgument();
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    if (Enum.TryParse<Animation>(animationName, out var animation))
        //    {
        //        try
        //        {
        //            _chatBox.OutputTo(player, $"Started animation '{animation}'");
        //            await playerElementComponent.DoAnimationAsync(animation);
        //            _chatBox.OutputTo(player, $"Finished animation '{animation}'");
        //        }
        //        catch (NotSupportedException)
        //        {
        //            _chatBox.OutputTo(player, $"Animation '{animationName}' is not supported");
        //        }
        //    }
        //    else
        //        _chatBox.OutputTo(player, $"Animation '{animationName}' not found.");

        //});

        //_commandService.AddAsyncCommandHandler("complexanimation", async (player, args) =>
        //{
        //    var animationName = args.ReadArgument();
        //    if (Enum.TryParse<Animation>(animationName, out var animation))
        //    {
        //        try
        //        {
        //            await player.GetRequiredComponent<PlayerElementComponent>().DoComplexAnimationAsync(animation, true);
        //        }
        //        catch (NotSupportedException)
        //        {
        //            _chatBox.OutputTo(player, $"Animation '{animationName}' is not supported");
        //        }
        //    }
        //    else
        //        _chatBox.OutputTo(player, $"Animation '{animationName}' not found.");
        //});

        //_commandService.AddCommandHandler("mygroups", (player, args) =>
        //{
        //    _chatBox.OutputTo(player, "Groups:");
        //    foreach (var item in playerLists.OfType<GroupMemberComponent>())
        //    {
        //        _chatBox.OutputTo(player, $"Group id: {item.GroupId}, rank: {item.Rank}, rank name: '{item.RankName}'");
        //    }
        //});

        //_commandService.AddCommandHandler("myfractions", (player, args) =>
        //{
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    _chatBox.OutputTo(player, "Fractions:");
        //    foreach (var item in player.OfType<FractionMemberComponent>())
        //    {
        //        _chatBox.OutputTo(player, $"Fraction id: {item.FractionId}, rank: {item.Rank}, rank name: '{item.RankName}'");
        //    }
        //});

        //_commandService.AddCommandHandler("createvehiclehud", (player, args) =>
        //{
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    player.AddComponent(new SampleVehicleHud(assetsRegistry));
        //});

        _commandService.AddAsyncCommandHandler("createhud", async (player, args, token) =>
        {
            var hud = player.Hud.AddLayer<SampleHudLayer>();
            if (hud == null)
                return;

            await Task.Delay(1000, token);
            hud.Offset = new Vector2(0, 100);
        });

        _commandService.AddCommandHandler("destroyHud", (player, args) =>
        {
            player.Hud.RemoveLayer<SampleHudLayer>();
            player.Hud.RemoveLayer<SampleHud2Layer>();
            player.Hud.RemoveLayer<SampleStatefulHudLayer>();
        });

        _commandService.AddCommandHandler("createHud2", (player, args) =>
        {
            var hud = player.Hud.AddLayer<SampleStatefulHudLayer>();
        });
        
        _commandService.AddCommandHandler("removeHud2", (player, args) =>
        {
            player.Hud.TryRemoveLayer<SampleStatefulHudLayer>();
        });
        
        _commandService.AddCommandHandler("setStateHud2", (player, args) =>
        {
            if(player.Hud.TryGetLayer(out SampleStatefulHudLayer layer))
            {
                layer.Update();
            }
        });

        //_commandService.AddCommandHandler("createhud3", async (player, args) =>
        //{
        //    var hud = player.AddComponent(new SampleHud3());
        //    while (true)
        //    {
        //        await Task.Delay(1000);
        //        hud.Update();
        //    }
        //});

        //_commandService.AddCommandHandler("updatestate2", (player, args) =>
        //{
        //    var sampleHud2 = player.GetRequiredComponent<SampleHud2>();
        //    sampleHud2.Update();
        //});

        //_commandService.AddCommandHandler("movehud", (player, args) =>
        //{
        //    var sampleHud = player.GetRequiredComponent<SampleHud>();
        //    sampleHud.Position = new Vector2(0, _hudPosition++ * 10);
        //});

        //_commandService.AddCommandHandler("createstatefulhud", (player, args) =>
        //{
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    var sampleHud = player.AddComponent(new SampleStatefulHud(new SampleHudState
        //    {
        //        Text1 = "text1",
        //        Text2 = "text2",
        //    }, assetsRegistry));
        //});

        //_commandService.AddCommandHandler("updatestate", (player, args) =>
        //{
        //    var sampleHud = player.GetRequiredComponent<SampleStatefulHud>();
        //    sampleHud.Update();
        //});

        //_commandService.AddCommandHandler("destroyhuds", (player, args) =>
        //{
        //    player.TryDestroyComponent<SampleHud>();
        //    player.TryDestroyComponent<SampleStatefulHud>();
        //});

        //_commandService.AddCommandHandler("discord", (player, args) =>
        //{
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    if (player.HasComponent<DiscordIntegrationComponent>())
        //    {
        //        _chatBox.OutputTo(player, "Twoje konto jest już połączone z discordem.");
        //    }
        //    player.TryDestroyComponent<PendingDiscordIntegrationComponent>();
        //    var pendingDiscordIntegrationComponent = new PendingDiscordIntegrationComponent(dateTimeProvider);
        //    var code = pendingDiscordIntegrationComponent.GenerateAndGetDiscordConnectionCode();
        //    player.AddComponent(pendingDiscordIntegrationComponent);
        //    _chatBox.OutputTo(player, $"Aby połączyć konto wpisz na kanale discord #polacz-konto komendę: /polaczkonto {code}");
        //});

        //_commandService.AddCommandHandler("adduserupgrade", (player, args) =>
        //{
        //    var userComponent = player.GetRequiredComponent<UserComponent>();
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    var i = Random.Shared.Next(0, 10);
        //    if (userComponent.TryAddUpgrade(i))
        //        _chatBox.OutputTo(player, $"Pomyślnie dodano ulepszenie id {i}");
        //    else
        //        _chatBox.OutputTo(player, $"Pomyślnie dodano ulepszenie id {i}");
        //});

        //_commandService.AddAsyncCommandHandler("ban", async (player, args) =>
        //{
        //    var userComponent = player.GetRequiredComponent<UserComponent>();
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    await _banService.Ban(player);
        //    playerElementComponent.Kick("test 123");
        //});

        //_commandService.AddAsyncCommandHandler("amibanned", async (player, args) =>
        //{
        //    var userComponent = player.GetRequiredComponent<UserComponent>();
        //    var isBanned = userComponent.Bans.IsBanned(_dateTimeProvider.Now, 123);
        //    _chatBox.OutputTo(player, $"isBanned {isBanned}");
        //});

        //_commandService.AddAsyncCommandHandler("bantest", async (player, args) =>
        //{
        //    var userComponent = player.GetRequiredComponent<UserComponent>();
        //    await _banService.Ban(player, type: 123);
        //    var isBanned = userComponent.Bans.IsBanned(_dateTimeProvider.Now, 123);
        //    _chatBox.OutputTo(player, $"isBanned {isBanned}");
        //});

        //_commandService.AddAsyncCommandHandler("unbantest", async (player, args) =>
        //{
        //    var userComponent = player.GetRequiredComponent<UserComponent>();
        //    await _banService.RemoveBan(player, 123);
        //    var isBanned = userComponent.Bans.IsBanned(_dateTimeProvider.Now, 123);
        //    _chatBox.OutputTo(player, $"isBanned {isBanned}");
        //});

        //_commandService.AddCommandHandler("attach", (player, args) =>
        //{
        //    var objectEntity = _elementFactory.CreateObject((ObjectModel)1337, Vector3.Zero, Vector3.Zero);
        //    //player.AddComponent(new AttachedEntityComponent(objectEntity, SlipeServer.Packets.Enums.BoneId.Pelvis1, new Vector3(-1, -1, 1)));
        //    player.AddComponent(new OwnerDisposableComponent(objectEntity));
        //    objectEntity.Disposed += e =>
        //    {
        //        logger.LogInformation("Disposed attached player");
        //    };
        //});

        //_commandService.AddCommandHandler("destroyattachedentity", (player, args) =>
        //{
        //    var userComponent = player.GetRequiredComponent<UserComponent>();
        //    var attachedEntity = player.GetRequiredComponent<AttachedElementComponent>();
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    attachedEntity.AttachedEntity.Dispose();
        //    if (player.HasComponent<AttachedElementComponent>())
        //    {
        //        _chatBox.OutputTo(player, "Nie udalo sie zniszczyc");
        //    }
        //    else
        //    {
        //        _chatBox.OutputTo(player, "Zniszczone");
        //    }
        //});

        //_commandService.AddCommandHandler("testrole", (player, args) =>
        //{
        //    var userComponent = player.GetRequiredComponent<UserComponent>();
        //    var isAdmin = userComponent.IsInRole("admin");
        //    var roles = userComponent.GetRoles();
        //    foreach (var item in roles)
        //    {
        //        if (!userComponent.IsInRole(item))
        //        {
        //            throw new Exception();
        //        }
        //    }
        //});

        //_commandService.AddCommandHandler("testlogs", (player, args) =>
        //{
        //    _logger.LogInformation("test test 1");
        //    var activity = new Activity("TestLogsActivity");
        //    activity.Start();
        //    _logger.LogInformation("test test 2");
        //    activity.Stop();
        //});

        //_commandService.AddCommandHandler("nametags", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var ped = _elementFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, player.Position + new Vector3(4, 0, 0));
        //    ped.AddComponent(new NametagComponent("[22] Borsuk"));
        //});

        //_commandService.AddAsyncCommandHandler("nametags2", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var nametag = new NametagComponent("[22] Borsuk");
        //    var ped = _elementFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, player.Position + new Vector3(4, 0, 0));
        //    ped.AddComponent(nametag);
        //    await Task.Delay(1000);
        //    ped.DestroyComponent(nametag);
        //});

        //_commandService.AddAsyncCommandHandler("nametags3", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var nametag = new NametagComponent("[22] Borsuk");
        //    var ped = _elementFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, player.Position + new Vector3(4, 0, 0));
        //    ped.AddComponent(nametag);
        //    await Task.Delay(1000);
        //    nametag.Text = "[100] Borsuk";
        //});

        //_commandService.AddCommandHandler("nametags4", (player, args) =>
        //{
        //    nametagsService.SetNametagRenderingEnabled(player, args.ReadArgument() == "true");
        //});

        //_commandService.AddAsyncCommandHandler("nametags5", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var nametag = new NametagComponent("[22] Borsuk");
        //using var ped = _elementFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, player.Position + new Vector3(4, 0, 0));
        //    ped.AddComponent(nametag);
        //    await Task.Delay(1000);
        //});

        //_commandService.AddCommandHandler("nametags6", (player, args) =>
        //{
        //    nametagsService.SetLocalPlayerRenderingEnabled(player, args.ReadArgument() == "true");
        //});

        //_commandService.AddCommandHandler("outlinerendering", (player, args) =>
        //{
        //    elementOutlineService.SetRenderingEnabled(player, args.ReadArgument() == "true");
        //});

        //_commandService.AddCommandHandler("outline1", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var ped = _elementFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, player.Position + new Vector3(4, 0, 0));
        //    var @object = _elementFactory.CreateObject((ObjectModel)1337, player.Position + new Vector3(4, 4, 0), Vector3.Zero);
        //    ped.AddComponent(new OutlineComponent(Color.Red));
        //    @object.AddComponent(new OutlineComponent(Color.Red));
        //});

        //_commandService.AddAsyncCommandHandler("outline2", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //using var ped = _elementFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, player.Position + new Vector3(4, 0, 0));
        //    ped.AddComponent(new OutlineComponent(Color.Red));
        //    await Task.Delay(1000);
        //});

        //_commandService.AddAsyncCommandHandler("outline3", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var ped = _elementFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, player.Position + new Vector3(4, 0, 0));
        //    var outlineComponent = ped.AddComponent(new OutlineComponent(Color.Red));
        //    await Task.Delay(1000);
        //    ped.DestroyComponent(outlineComponent);
        //});

        //_commandService.AddCommandHandler("outline4", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var ped = _elementFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Truth, player.Position + new Vector3(4, 0, 0));
        //    var @object = _elementFactory.CreateObject((ObjectModel)1337, player.Position + new Vector3(4, 4, 0), Vector3.Zero);
        //    elementOutlineService.SetEntityOutlineForPlayer(player, ped, Color.Red);
        //    elementOutlineService.SetEntityOutlineForPlayer(player, @object, Color.Blue);
        //});

        //_commandService.AddCommandHandler("randomvehcolor", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var rnd = Random.Shared;
        //    var veh = player.GetRequiredComponent<PlayerElementComponent>().Vehicle.UpCast().GetRequiredComponent<VehicleElementComponent>();
        //    veh.Colors.Primary = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        //    veh.Colors.Secondary = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        //    veh.Colors.Color3 = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        //    veh.Colors.Color4 = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        //});

        //_commandService.AddAsyncCommandHandler("whitelistmyserial", async (player, args) =>
        //{
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    var userComponent = player.GetRequiredComponent<UserComponent>();

        //    if (await _userWhitelistedSerialsRepository.TryAddWhitelistedSerial(userComponent.Id, playerElementComponent.Client.Serial))
        //    {
        //        _chatBox.OutputTo(player, $"Dodano serial");
        //    }
        //    else
        //    {
        //        _chatBox.OutputTo(player, $"Nie udało się dodać");
        //    }
        //});

        //_commandService.AddAsyncCommandHandler("removewhitelistmyserial", async (player, args) =>
        //{
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    var userComponent = player.GetRequiredComponent<UserComponent>();

        //    if (await _userWhitelistedSerialsRepository.TryRemoveWhitelistedSerial(userComponent.Id, playerElementComponent.Client.Serial))
        //    {
        //        _chatBox.OutputTo(player, $"Usunięto serial");
        //    }
        //    else
        //    {
        //        _chatBox.OutputTo(player, $"Nie udało się usunąć");
        //    }
        //});

        //_commandService.AddCommandHandler("addvisualupgrade", (player, args) =>
        //{
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    if (playerElementComponent.Vehicle.UpCast().GetRequiredComponent<VehicleUpgradesComponent>().AddUniqueUpgrade(3))
        //    {
        //        _chatBox.OutputTo(player, $"dodano wizualne ulepszenie");
        //    }
        //    else
        //    {
        //        _chatBox.OutputTo(player, $"Nie udało się dodać wizualnego ulepszenia");
        //    }
        //});

        //_commandService.AddCommandHandler("addinvalidguicomponent", (player, args) =>
        //{
        //    player.AddComponent<InvalidGuiComponent>();
        //});

        //_commandService.AddCommandHandler("text3dcomp", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var markerEntity = elementFactory.CreateMarker(MarkerType.Arrow, player.Position, Color.White);
        //    markerEntity.AddComponent(new Text3dComponent("test1", player.Position));
        //    markerEntity.AddComponent(new Text3dComponent("offset z+1", player.Position + new Vector3(0, 0, 1)));
        //});

        //_commandService.AddCommandHandler("disposeveh", (player, args) =>
        //{
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    playerElementComponent.Vehicle.Destroy();
        //});

        _commandService.AddCommandHandler("createtestpickups", (player, args) =>
        {
            var pickup1 = _elementFactory.CreatePickup(new Location(new Vector3(340.4619f, -131.87402f, 1.578125f)), 1274);
            var pickup2 = _elementFactory.CreatePickup(new Location(new Vector3(340.4619f, -131.87402f, 1.578125f), Vector3.Zero, Interior: 1), 1274);
            var pickup3 = _elementFactory.CreatePickup(new Location(new Vector3(340.4619f, -131.87402f, 1.578125f), Vector3.Zero, Dimension: 1), 1274);

            pickup1.CollisionShape.ElementEntered += (pickup, element) =>
            {
                _chatBox.Output("Enter pickup 1");
            };

            pickup2.CollisionShape.ElementEntered += (pickup, element) =>
            {
                _chatBox.Output("Enter pickup 2");
            };

            pickup3.CollisionShape.ElementEntered += (pickup, element) =>
            {
                _chatBox.Output("Enter pickup 3");
            };
        });

        _commandService.AddCommandHandler("interior", (player, args) =>
        {
            player.Interior = args.ReadByte();
        });
        
        _commandService.AddCommandHandler("dimension", (player, args) =>
        {
            player.Dimension = args.ReadUShort();
        });

        //_commandService.AddCommandHandler("inventoryoccupied", (player, args) =>
        //{
        //    var inv = player.GetRequiredComponent<inventory>();
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    _chatBox.OutputTo(player, $"Inventory: {inv.Number}/{inv.Size}");
        //});


        //_commandService.AddCommandHandler("itemwithmetadata", (player, args) =>
        //{
        //    var inv = player.GetRequiredComponent<inventory>();
        //    var item = inv.AddSingleItem(_itemsRegistry, 4, new Metadata
        //    {
        //        ["number"] = 1m
        //    });

        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    _chatBox.OutputTo(player, $"Item regular: {item.GetMetadata("number").GetType()}");
        //    _chatBox.OutputTo(player, $"Item cast<int>: {item.GetMetadata<int>("number").GetType()}");
        //});

        //_commandService.AddCommandHandler("proceduralobject", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var objectEntity = _elementFactory.CreateObject(ObjectModel.Gunbox, player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero);
        //});

        //_commandService.AddCommandHandler("day", (player, args) =>
        //{
        //    gameWorld.SetTime(12, 0);
        //});

        //_commandService.AddCommandHandler("night", (player, args) =>
        //{
        //    gameWorld.SetTime(0, 0);
        //});

        //_commandService.AddCommandHandler("createObjectFor", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //using var scopedelementFactory = _elementFactory.CreateScopedelementFactory(player);
        //    scopedelementFactory.CreateObject((ObjectModel)1337, player.Position + new Vector3(3, 0, 0), player.Rotation);
        //});

        //_commandService.AddCommandHandler("tp", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    player.Position = new Vector3(0, 0, 3);
        //});

        //_commandService.AddCommandHandler("i", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    player.Interior = args.ReadByte();
        //});

        //_commandService.AddCommandHandler("d", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    player.Dimension = args.ReadUShort();
        //});

        //_commandService.AddCommandHandler("runtimeobject", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var modelFactory = new ModelFactory();
        //    modelFactory.AddTriangle(new Vector3(2, 2, 0), new Vector3(0, 10, 0), new Vector3(10, 0, 0), "Metal1_128");
        //    modelFactory.AddTriangle(new Vector3(10, 0, 0), new Vector3(0, 10, 0), new Vector3(10, 10, 0), "Metal1_128");
        //    var dff = modelFactory.BuildDff();
        //    var col = modelFactory.BuildCol();
        //    assetsService.ReplaceModelFor(player, dff, col, 1339);
        //    _elementFactory.CreateObject((ObjectModel)1339, player.Position + new Vector3(15, 15, -5), Vector3.Zero);
        //});


        //_commandService.AddCommandHandler("restoreobject", (player, args) =>
        //{
        //    assetsService.RestoreModelFor(player, 1339);
        //});

        //_commandService.AddCommandHandler("amiinwater", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    _chatBox.OutputTo(player, $"amiinwater: {playerElementComponent.IsInWater} {player.Position}");
        //});

        //_commandService.AddCommandHandler("formatmoney", (player, args) =>
        //{
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    _chatBox.OutputTo(player, $"{123.123m.FormatMoney(new System.Globalization.CultureInfo("pl-PL"))}");
        //});

        //_commandService.AddAsyncCommandHandler("createObjectFor2", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //using var scopedelementFactory = _elementFactory.CreateScopedelementFactory(player);
        //    var pos = player.Position + new Vector3(3, 0, 0);

        //    void handleComponentCreated(IScopedElementFactory scopedelementFactory, PlayerPrivateElementComponentBase playerPrivateElementComponentBase)
        //    {
        //        ;
        //    }
        //    scopedelementFactory.ComponentCreated += handleComponentCreated;
        //    scopedelementFactory.CreateObject((ObjectModel)1337, pos, player.Rotation);
        //    for (int i = 0; i < 10; i++)
        //    {
        //        await Task.Delay(2000);
        //        //obj.Position = pos + new Vector3(i, 0, 0);
        //        //obj.Rotation = player.Rotation;
        //    }
        //});

        //_commandService.AddCommandHandler("addtestmarker1", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    spawnMarkersService.AddSpawnMarker(new PointSpawnMarker("test123", player.Position));
        //    _chatBox.OutputTo(player, "marker added1");
        //});

        //_commandService.AddCommandHandler("addtestmarker2", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    spawnMarkersService.AddSpawnMarker(new DirectionalSpawnMarker("test direct", player.Position, player.Rotation.Z));
        //    _chatBox.OutputTo(player, "marker added2");
        //});

        //_commandService.AddCommandHandler("testnotrace", (player, args) =>
        //{
        //    _logger.LogInformation("no trace");
        //}, null);

        //int counter = 0;
        //_commandService.AddCommandHandler("counter", (player, args) =>
        //{
        //    counter++;
        //    _logger.LogInformation("Counter: {counter}", counter);
        //});

        //_commandService.AddCommandHandler("level", (player, args) =>
        //{
        //    var levelComponent = player.GetRequiredComponent<LevelComponent>();
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    _chatBox.OutputTo(player, $"Level: {levelComponent.Level}, exp: {levelComponent.Experience}");
        //});

        //_commandService.AddCommandHandler("setlevel", (player, args) =>
        //{
        //    uint level = args.ReadUInt();
        //    var levelComponent = player.GetRequiredComponent<LevelComponent>();
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    void handleLevelChanged(LevelComponent that, uint level, bool up)
        //    {
        //        _chatBox.OutputTo(player, $"Level change: {level}, {up}");
        //    }
        //    levelComponent.LevelChanged += handleLevelChanged;
        //    levelComponent.Level = level;
        //    levelComponent.LevelChanged -= handleLevelChanged;
        //});

        _commandService.AddCommandHandler("devtools", (player, args) =>
        {
            var browser = player.Browser;
            player.Admin.DevelopmentMode = true;
            browser.DevTools = !browser.DevTools;
            _chatBox.OutputTo(player, $"Devtools {browser.DevTools}");
        }, null);

        //_commandService.AddCommandHandler("browserpath", (player, args) =>
        //{
        //    var browserComponent = player.GetRequiredComponent<BrowserComponent>();
        //    _chatBox.OutputTo(player, $"Path {browserComponent.Path}");
        //}, null);

        //_commandService.AddAsyncCommandHandler("setkind", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    await _vehiclesService.SetVehicleKind(player.GetRequiredComponent<PlayerElementComponent>().Vehicle.UpCast(), 42);
        //});

        //_commandService.AddAsyncCommandHandler("kind", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    var veh = playerElementComponent.Vehicle;
        //    var kind = veh.UpCast().GetRequiredComponent<PrivateVehicleComponent>().Kind;
        //    _chatBox.OutputTo(player, $"Kind: {kind}");
        //});

        //_commandService.AddAsyncCommandHandler("counterasync", async (player, args) =>
        //{
        //    counter++;
        //    _logger.LogInformation("Counter: {counter}", counter);
        //});

        //_commandService.AddAsyncCommandHandler("addvehevent", async (player, args) =>
        //{
        //    await _vehiclesService.AddVehicleEvent(player.GetRequiredComponent<PlayerElementComponent>().Vehicle.UpCast(), 1);
        //});

        //_commandService.AddAsyncCommandHandler("vehevents", async (player, args) =>
        //{
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    var events = await _vehiclesService.GetAllVehicleEvents(playerElementComponent.Vehicle.UpCast());
        //    _chatBox.OutputTo(player, "Events:");
        //    foreach (var item in events)
        //    {
        //        _chatBox.OutputTo(player, $"Event: {item.DateTime} - {item.EventType}");
        //    }
        //});

        //_commandService.AddCommandHandler("browserguiopen", (player, args) =>
        //{
        //    player.GetRequiredComponent<BrowserComponent>().Path = "Counter";
        //});

        //_commandService.AddCommandHandler("browserguiclose", (player, args) =>
        //{
        //    player.GetRequiredComponent<BrowserComponent>().Path = null;
        //});

        //_commandService.AddCommandHandler("kickme", (player, args) =>
        //{
        //    usersService.Kick(player, "test");
        //});

        //_commandService.AddCommandHandler("getplayerbyname", (player, args) =>
        //{
        //    if (usersService.TryGetPlayerByName(args.ReadArgument(), out var foundPlayer))
        //    {
        //        _chatBox.OutputTo(player, "found");
        //    }
        //    else
        //        _chatBox.OutputTo(player, "not found");
        //});

        //_commandService.AddCommandHandler("findbyname", (player, args) =>
        //{
        //    var players = usersService.SearchPlayersByName(args.ReadArgument());
        //    _chatBox.OutputTo(player, "found:");
        //    foreach (var item in players)
        //    {
        //        _chatBox.OutputTo(player, $"Player: {item.GetRequiredComponent<UserComponent>().UserName}");
        //    }
        //});

        //_commandService.AddAsyncCommandHandler("addrating", async (player, args) =>
        //{
        //    var last = await feedbackService.GetLastRating(player, 1) ?? (0, DateTime.MinValue);
        //    if (last.Item2.AddSeconds(3) > dateTimeProvider.Now)
        //    {
        //        _chatBox.OutputTo(player, "możesz ocenić maksymalnie raz na 30sekund");
        //        return;
        //    }
        //    var rating = Random.Shared.Next(100);
        //    await feedbackService.ChangeLastRating(player, 1, rating);
        //    _chatBox.OutputTo(player, $"zmieniono ocenę z {rating} z {last.Item1}");
        //});

        //_commandService.AddAsyncCommandHandler("addopinion", async (player, args) =>
        //{
        //    await feedbackService.AddOpinion(player, 1, string.Join(", ", args));
        //    _chatBox.OutputTo(player, "Opinia dodana");
        //});

        //_commandService.AddCommandHandler("addprivatemarker", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    void handleEntityEntered(MarkerElementComponent markerElementComponent, Entity enteredMarker, Entity enteredEntity)
        //    {
        //        player.DestroyComponent(markerElementComponent);
        //    }

        //using var scopedelementFactory = _elementFactory.CreateScopedelementFactory(player);
        //    scopedelementFactory.CreateMarker(MarkerType.Checkpoint, player.Position with { X = player.Position.X + 4 }, Color.White);
        //    var marker = scopedelementFactory.LastCreatedComponent as PlayerPrivateElementComponent<MarkerElementComponent>;
        //    marker.ElementComponent.EntityEntered = handleEntityEntered;
        //});

        //_commandService.AddAsyncCommandHandler("createmarkerforme", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //using var scopedelementFactory = _elementFactory.CreateScopedelementFactory(player);
        //    scopedelementFactory.CreateMarker(MarkerType.Cylinder, player.Position, Color.White);
        //    var component = scopedelementFactory.LastCreatedComponent as PlayerPrivateElementComponent<MarkerElementComponent>;
        //    component.ElementComponent.Size = 4;
        //    component.ElementComponent.Color = Color.Red;
        //    while (true)
        //    {
        //        if (component.ElementComponent.Size == 4)
        //        {
        //            component.ElementComponent.Size = 2;
        //            component.ElementComponent.Color = Color.Red;
        //        }
        //        else
        //        {
        //            component.ElementComponent.Size = 4;
        //            component.ElementComponent.Color = Color.Blue;
        //        }
        //        await Task.Delay(500);
        //    }
        //});

        //_commandService.AddAsyncCommandHandler("createmarkerforme2", async (player, args) =>
        //{
        //using var scopedelementFactory = _elementFactory.CreateScopedelementFactory(player);
        //    scopedelementFactory.CreateMarker(MarkerType.Cylinder, new Vector3(-600.8877f, 240.88867f, 26.091864f), Color.White);
        //    var marker = scopedelementFactory.LastCreatedComponent as PlayerPrivateElementComponent<MarkerElementComponent>;
        //    marker.ElementComponent.Size = 4;
        //    marker.ElementComponent.Color = Color.Red;
        //});

        //_commandService.AddCommandHandler("markerhittest1", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var markerEntity = _elementFactory.CreateMarker(MarkerType.Cylinder, player.Position, Color.White);
        //    var marker = markerEntity.GetRequiredComponent<MarkerElementComponent>();
        //    marker.Size = 4;
        //    marker.Color = Color.Red;
        //    marker.EntityEntered = (markerElementComponent, enteredMarker, enteredEntity) =>
        //    {
        //        Console.WriteLine("player entered (public marker)");
        //    };
        //});

        //_commandService.AddCommandHandler("markerhittest2", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //using var scopedelementFactory = _elementFactory.CreateScopedelementFactory(player);
        //    scopedelementFactory.CreateMarker(MarkerType.Cylinder, player.Position, Color.White);
        //    var marker = scopedelementFactory.LastCreatedComponent as PlayerPrivateElementComponent<MarkerElementComponent>;
        //    marker.ElementComponent.Size = 4;
        //    marker.ElementComponent.Color = Color.Red;
        //    marker.ElementComponent.EntityEntered = (markerElementComponent, enteredMarker, enteredEntity) =>
        //    {
        //        Console.WriteLine("player entered (private marker)");
        //    };
        //});
        //_commandService.AddCommandHandler("setinterior", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    if (player.Interior == 1)
        //        player.Interior = 0;
        //    else
        //        player.Interior = 1;
        //});

        //_commandService.AddCommandHandler("setinterior2", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    player.Interior = 1;
        //    var veh = _elementFactory.CreateVehicle(404, new Vector3(338.26562f, -87.75098f, 1.5197021f), Vector3.Zero);
        //    player.GetRequiredComponent<PlayerElementComponent>().WarpIntoVehicle(veh.GetRequiredComponent<VehicleElementComponent>());
        //});

        //_commandService.AddAsyncCommandHandler("setinterior2b", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();

        //    void handleInteriorChanged(Element sender, SlipeServer.Server.Elements.Events.ElementChangedEventArgs<byte> args)
        //    {
        //        chatBox.OutputTo(player, $"Changed interior to: {args.NewValue}");
        //    }

        //    player.InteriorChanged += handleInteriorChanged;
        //    player.Interior = 1;
        //    var veh = _elementFactory.CreateVehicle(404, new Vector3(338.26562f, -87.75098f, 1.5197021f), Vector3.Zero);
        //    await Task.Delay(1000);
        //    player.GetRequiredComponent<PlayerElementComponent>().WarpIntoVehicle((Vehicle)veh.GetRequiredComponent<IElementComponent>());
        //});

        //_commandService.AddCommandHandler("setinterior3", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    player.Interior = 1;
        //    player.Interior = 0;
        //});

        //_commandService.AddCommandHandler("position", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    _chatBox.OutputTo(player, $"Position: {player.Position}, rot: {player.Rotation}, i: {player.Interior}, d: {player.Dimension}");
        //});

        //_commandService.AddCommandHandler("testargs", (player, args) =>
        //{
        //    var a = args.ReadInt();
        //    var b = args.ReadByte();
        //    var c = args.ReadInt();
        //    _chatBox.OutputTo(player, $"Komenda wykonana, argumenty {a}, {b}, {c}");
        //});

        //_commandService.AddCommandHandler("testargs2", (player, args) =>
        //{
        //    var a = args.ReadPlayer();
        //    var b = args.ReadPlayer();
        //    _chatBox.OutputTo(player, $"Komenda wykonana, argumenty {a} {b}");
        //});

        //_commandService.AddCommandHandler("admincmd", (player, args) =>
        //{
        //    _chatBox.OutputTo(player, $"executed admin cmd");
        //}, new string[] { "Admin" });

        //_commandService.AddCommandHandler("enum", (player, args) =>
        //{
        //    _chatBox.OutputTo(player, $"Enum value: {args.ReadEnum<TestEnum>()}");
        //}, new string[] { "Admin" });

        _commandService.AddCommandHandler("guitest1", (player, args) =>
        {
            if (player.Gui.Current == null)
                player.Gui.Current = new Counter1Gui(player);
            else
                player.Gui.Current = null;
        });

        //_commandService.AddAsyncCommandHandler("guitest2", async (player, args) =>
        //{
        //    player.TryDestroyComponent<BrowserGuiComponent>();
        //    player.AddComponent<Counter2GuiComponent>();
        //    _chatBox.OutputTo(player, "Loaded counter 2");
        //});

        commandService.AddCommandHandler("browserloadcounter1", (player, args) =>
        {
            if (player.Browser.Visible)
            {
                player.Browser.Visible = false;
                return;
            }
            player.Browser.Open("/realmUi/counter1");
            _chatBox.OutputTo(player, "Loaded counter 1");
        });
        //_commandService.AddAsyncCommandHandler("browserloadcounter2", async (player, args) =>
        //{
        //    var browserComponent = player.GetRequiredComponent<BrowserComponent>();
        //    browserComponent.Close();
        //    browserComponent.Path = "/realmUi/counter2";
        //    browserComponent.Visible = true;
        //    _chatBox.OutputTo(player, "Loaded counter 2");
        //});
        //_commandService.AddAsyncCommandHandler("navigatetest", async (player, args) =>
        //{
        //    var browserComponent = player.GetRequiredComponent<BrowserComponent>();
        //    browserComponent.Path = "/realmUi/counter2";
        //    _chatBox.OutputTo(player, "navigated");
        //});
        //_commandService.AddCommandHandler("browserinteractive", (player, args) =>
        //{
        //    if (!player.TryDestroyComponent<InteractiveGuiComponent>())
        //    {
        //        player.AddComponent<InteractiveGuiComponent>();
        //        _chatBox.OutputTo(player, "Loaded InteractiveGuiComponent");
        //    }
        //});
        //_commandService.AddAsyncCommandHandler("removebrowserinteractive", async (player, args) =>
        //{
        //    player.TryDestroyComponent<InteractiveGuiComponent>();
        //    _chatBox.OutputTo(player, "Destroyed InteractiveGuiComponent");
        //});
        //_commandService.AddAsyncCommandHandler("setfoo", async (player, args) =>
        //{
        //    player.GetRequiredComponent<InteractiveGuiComponent>().SetFoo(Guid.NewGuid().ToString());
        //    _chatBox.OutputTo(player, "Foo set");
        //});

        //_commandService.AddCommandHandler("browserloadindex", (player, args) =>
        //{
        //    player.GetRequiredComponent<BrowserComponent>().Path = "/";
        //    _chatBox.OutputTo(player, "Loaded /");
        //});

        //_commandService.AddCommandHandler("openbrowser", (player, args) =>
        //{
        //    player.GetRequiredComponent<BrowserComponent>().Visible = true;
        //    _chatBox.OutputTo(player, "Open");
        //});
        //_commandService.AddCommandHandler("closebrowser", (player, args) =>
        //{
        //    var browserComponent = player.GetRequiredComponent<BrowserComponent>();
        //    browserComponent.Path = "/realmEmpty";
        //    browserComponent.Visible = false;
        //    _chatBox.OutputTo(player, "Closed");
        //});

        //_commandService.AddCommandHandler("collisionshapes", (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var col1 = _elementFactory.CreateCollisionSphere(player.Position + new Vector3(6, 0, 0), 3);
        //    var col2 = _elementFactory.CreateCollisionSphere(player.Position + new Vector3(-6, 0, 0), 3);

        //    var collisionSphere1 = col1.GetRequiredComponent<CollisionSphereElementComponent>();
        //    var collisionSphere2 = col2.GetRequiredComponent<CollisionSphereElementComponent>();

        //    //collisionSphere1.EntityEntered += (a, b) =>
        //    //{
        //    //    _chatBox.OutputTo(player, "Entered collisionSphere 1");
        //    //};
        //    //collisionSphere2.EntityEntered += (a, b) =>
        //    //{
        //    //    _chatBox.OutputTo(player, "Entered collisionSphere 2");
        //    //};
        //    //collisionSphere1.EntityLeft += (a, b) =>
        //    //{
        //    //    _chatBox.OutputTo(player, "Left collisionSphere 1");
        //    //};
        //    //collisionSphere2.EntityLeft += (a, b) =>
        //    //{
        //    //    _chatBox.OutputTo(player, "Left collisionSphere 2");
        //    //};
        //});

        //_commandService.AddAsyncCommandHandler("collisionshapes2", async (player, args) =>
        //{
        //    var player = player.GetRequiredComponent<PlayerElementComponent>();
        //    var pos1 = player.Position + new Vector3(6, 0, 0);
        //    var pos2 = player.Position + new Vector3(-6, 0, 0);
        //    var col1 = _elementFactory.CreateCollisionSphere(pos1, 3);
        //    var col2 = _elementFactory.CreateCollisionSphere(pos2, 3);

        //    var collisionSphere1 = col1.GetRequiredComponent<CollisionSphereElementComponent>();
        //    var collisionSphere2 = col2.GetRequiredComponent<CollisionSphereElementComponent>();

        //    //collisionSphere1.EntityEntered += (a, b) =>
        //    //{
        //    //    _chatBox.OutputTo(player, "Entered collisionSphere 1");
        //    //};
        //    //collisionSphere2.EntityEntered += (a, b) =>
        //    //{
        //    //    _chatBox.OutputTo(player, "Entered collisionSphere 2");
        //    //};
        //    //collisionSphere1.EntityLeft += (a, b) =>
        //    //{
        //    //    _chatBox.OutputTo(player, "Left collisionSphere 1");
        //    //};
        //    //collisionSphere2.EntityLeft += (a, b) =>
        //    //{
        //    //    _chatBox.OutputTo(player, "Left collisionSphere 2");
        //    //};

        //    int counter = 20;
        //    bool a = false;
        //    while (true)
        //    {
        //        a = !a;
        //        if (a)
        //        {
        //            player.Position = pos1;
        //        }
        //        else
        //        {
        //            player.Position = pos2;
        //        }
        //        await Task.Delay(500);
        //        if (counter-- == 0)
        //            break;
        //    }
        //});

        //_commandService.AddCommandHandler("privateelementdisposable", async (player, args) =>
        //{
        //using var scopedelementFactory = _elementFactory.CreateScopedelementFactory(player);
        //    scopedelementFactory.CreateMarker(MarkerType.Cylinder, player.Position, Color.White);
        //    var marker = scopedelementFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<MarkerElementComponent>>();
        //    marker.ElementComponent.Size = 4;
        //    marker.ElementComponent.Color = Color.Red;
        //    await Task.Delay(1000);
        //    player.DestroyComponent(marker);
        //    _chatBox.Output("destory marker");
        //});

        //_commandService.AddCommandHandler("focusedelements", (player, args) =>
        //{
        //    var playerElementComponent = player.GetRequiredComponent<PlayerElementComponent>();
        //    _chatBox.OutputTo(player, $"Focused player: {playerElementComponent.FocusedEntity}");
        //    if (playerElementComponent.FocusedEntity != null)
        //    {
        //        var focusableComponent = playerElementComponent.FocusedEntity.GetRequiredComponent<FocusableComponent>();
        //        foreach (var focusedPlayer in focusableComponent.FocusedPlayers)
        //        {
        //            _chatBox.OutputTo(player, $"Focused player: {focusedPlayer}");
        //        }
        //    }
        //});

        //_commandService.AddAsyncCommandHandler("testpolicy", async (player, args) =>
        //{
        //    bool authorized = await usersService.AuthorizePolicy(player.GetRequiredComponent<UserComponent>(), "Admin");
        //    _chatBox.OutputTo(player, $"authorized: {authorized}");
        //});

        _commandService.AddAsyncCommandHandler("signout", async (player, args, token) =>
        {
            await _usersService.LogOut(player, token);
        });

        //_commandService.AddAsyncCommandHandler("updateLastNewsRead", async (player, args) =>
        //{
        //    await _usersService.UpdateLastNewsRead(player);
        //});

        //_commandService.AddAsyncCommandHandler("addmoneyhistory1", async (player, args) =>
        //{
        //    await _userMoneyHistoryService.Add(player, 123, 1, "add 123");
        //    _chatBox.Output("Added 1");
        //});

        //_commandService.AddAsyncCommandHandler("addmoneyhistory2", async (player, args) =>
        //{
        //    await _userMoneyHistoryService.Add(player, -123, 2, "remove 123");
        //    _chatBox.Output("Added 2");
        //});

        //_commandService.AddAsyncCommandHandler("showhistory", async (player, args) =>
        //{
        //    var history = await _userMoneyHistoryService.Get(player);
        //    foreach (var item in history)
        //    {
        //        _chatBox.Output($"> {item.DateTime}: {item.CurrentBalance} - {item.Description}");

        //    }
        //});

        //_commandService.AddAsyncCommandHandler("clickedElement", async (player, args) =>
        //{
        //    var lastClickedElement = player.LastClickedElement;
        //    _chatBox.Output($"> {lastClickedElement}");
        //});


        _commandService.AddAsyncCommandHandler("scopedelements", async (player, args, token) =>
        {
        using var scope = player.GetRequiredService<IScopedElementFactory>().CreateScope();
            scope.CreateObject(new Location(player.Position + new Vector3(3, 0, 0), Vector3.Zero), (ObjectModel)1337);
            await Task.Delay(1000, token);
        });

        _commandService.AddCommandHandler("createelementsforme", (player, args) =>
        {
            player.ElementFactory.CreateObject(new Location(player.Position + new Vector3(3, 0, 0), Vector3.Zero), (ObjectModel)1337);
        });

        _commandService.AddCommandHandler("listevents", (player, args) =>
        {
            _chatBox.OutputTo(player, "Events:");
            foreach (var item in player.Events)
            {
                _chatBox.OutputTo(player, $"Event: {item.Id} - {item.EventType}: {item.Metadata}");
            }
        });
        _commandService.AddCommandHandler("addevent", (player, args) =>
        {
            player.Events.Add(1, args.ReadAllAsString());
            _chatBox.OutputTo(player, "Added");
        });

        _commandService.AddAsyncCommandHandler("fetchmoreevents", async (player, args, token) =>
        {
            var fetched = await player.Events.FetchMore(10, token);
            _chatBox.OutputTo(player, $"fetched {fetched.Length} more events");
        });

        _commandService.AddAsyncCommandHandler("wait", async (player, args, token) =>
        {
            await Task.Delay(5000, token);
        });

        _commandService.AddAsyncCommandHandler("asyncFadeCamera", async (player, args, token) =>
        {
            await using (await player.FadeCameraAsync(CameraFade.Out, 0.5f, token))
            {
                await Task.Delay(2000, token);
            }
        });

        _commandService.AddAsyncCommandHandler("warppedintovehicle", async (player, args, token) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation, Interior: 4, Dimension: 3), VehicleModel.Perennial);

            player.WarpIntoVehicle(vehicle);
        });

        _commandService.AddCommandHandler("searchplayers", (player, args) =>
        {
            var foundPlayers = args.SearchPlayers();
            foreach (var foundPlayer in foundPlayers)
            {
                _chatBox.OutputTo(player, $"Found player: {foundPlayer.Name}");
            }
        });

        _commandService.AddCommandHandler("searchVehicles", (player, args) =>
        {
            var foundVehicles = args.SearchVehicles();
            foreach (var foundVehicle in foundVehicles)
            {
                _chatBox.OutputTo(player, $"Found vehicle: {foundVehicle.Name}");
            }
        });

        _commandService.AddCommandHandler("select", (player, args) =>
        {
            var pattern = args.ReadArgument();
            var foundPlayers = args.SearchPlayers(pattern, PlayerSearchOption.All | PlayerSearchOption.AllowEmpty);
            var foundVehicles = args.SearchVehicles(pattern);
            int a = 0;
            foreach (var foundPlayer in foundPlayers)
            {
                if (player.SelectedElements.TryAdd(foundPlayer))
                    a++;
            }
            foreach (var foundVehicle in foundVehicles)
            {
                if (player.SelectedElements.TryAdd(foundVehicle))
                    a++;
            }
            _chatBox.OutputTo(player, $"Selected {a} elements.");
        });

        _commandService.AddCommandHandler("deselect", (player, args) =>
        {
            var pattern = args.ReadArgument();
            var foundPlayers = args.SearchPlayers(pattern, PlayerSearchOption.All | PlayerSearchOption.AllowEmpty);
            var foundVehicles = args.SearchVehicles(pattern);
            int a = 0;
            foreach (var foundPlayer in foundPlayers)
            {
                if (player.SelectedElements.TryRemove(foundPlayer))
                    a++;
            }
            foreach (var foundVehicle in foundVehicles)
            {
                if (player.SelectedElements.TryRemove(foundVehicle))
                    a++;
            }
            _chatBox.OutputTo(player, $"Deselected {a} elements.");
        });

        _commandService.AddCommandHandler("listselected", (player, args) =>
        {
            _chatBox.OutputTo(player, "Selected elements:");
            foreach (var element in player.SelectedElements)
            {
                _chatBox.OutputTo(player, $"Selected element: {element}");
            }
        });

        _commandService.AddCommandHandler("setcolor", (player, args) =>
        {
            var color = args.ReadEnum<KnownColor>();
            foreach (var element in player.SelectedElements)
            {
                if (element is RealmVehicle vehicle)
                {
                    vehicle.Colors.Primary = Color.FromKnownColor(color);
                }
            }
        });

        _commandService.AddCommandHandler("setbind", (player, args) =>
        {
            player.SetBind("p", (player, keyState) =>
            {
                if (keyState == SlipeServer.Server.Elements.Enums.KeyState.Down)
                {
                    foreach (var element in player.SelectedElements)
                        _elementOutlineService.SetElementOutline(element, Color.Green);
                }
                if (keyState == SlipeServer.Server.Elements.Enums.KeyState.Up)
                {
                    foreach (var element in player.SelectedElements)
                        _elementOutlineService.RemoveElementOutline(element);
                }
            });
        });

        _commandService.AddCommandHandler("scheduler1", (player, args) =>
        {
            player.Scheduler.ScheduleJobOnce(() =>
            {
                _chatBox.OutputTo(player, "Once");
                System.Console.WriteLine("Player once");
                return Task.CompletedTask;
            }, TimeSpan.FromSeconds(5));
        });

        _commandService.AddCommandHandler("scheduler2", (player, args) =>
        {
            player.Scheduler.ScheduleJob(() =>
            {
                _chatBox.OutputTo(player, "repeat");
                System.Console.WriteLine("Player repeat");
                return Task.CompletedTask;
            }, TimeSpan.FromSeconds(2));
        });

        _commandService.AddCommandHandler("setstat", (player, args) =>
        {
            player.Statistics.SetGtaSa(PedStat.BIKE_SKILL, 999);
            player.Statistics.SetGtaSa(PedStat.CYCLE_SKILL, 999);
        });

        _commandService.AddCommandHandler("playtimecategory", (player, args) =>
        {
            player.PlayTime.Category = args.ReadInt();
            _chatBox.OutputTo(player, $"Category changed to {player.PlayTime.Category}");
        });

        _commandService.AddCommandHandler("playtimelist", (player, args) =>
        {
            _chatBox.OutputTo(player, $"Play time list:");
            foreach (var item in player.PlayTime)
            {
                _chatBox.OutputTo(player, $"{item.Category} = {item.PlayTime}");
            }
        });

        _commandService.AddAsyncCommandHandler("toggleAllControlsScope", async (player, args, token) =>
        {
            using var _ = new ToggleAllControlsScope(player);

            await Task.Delay(500, token);
        });

        _commandService.AddCommandHandler("mapnameadd", (player, args) =>
        {
            var id1 = _mapNamesService.AddNameFor(new MapName("test1", Color.White, new Vector3(1000, 0, 0)), player);
            var id2 = _mapNamesService.AddNameFor(new MapName("test2", Color.Red, new Vector3(1000, 500, 0), Category: 1), player);
            var id3 = _mapNamesService.AddNameFor(new MapName("destroyed", Color.Red, new Vector3(1000, 1000, 0), Category: 1), player);
             _mapNamesService.RemoveFor(id3, player);
        });
        
        _commandService.AddAsyncCommandHandler("mapnameaddtemp", async (player, args, token) =>
        {
            var id = _mapNamesService.AddName(new MapName("temp name", Color.Yellow, new Vector3(-1000, 0, 0)));
            await Task.Delay(3000, token);
            _mapNamesService.Remove(id);
        });

        _commandService.AddAsyncCommandHandler("mapnameaddtemprename", async (player, args, token) =>
        {
            var id = _mapNamesService.AddName(new MapName("temp name", Color.Yellow, new Vector3(-1000, 0, 0)));
            for (int i = 0; i < 10; i++)
            {
                _mapNamesService.SetNameFor(id, $"new name {i}", player);
                await Task.Delay(1000, token);
            }
            _mapNamesService.Remove(id);
        });

        _commandService.AddCommandHandler("changecategories0", (player, args) =>
        {
            _mapNamesService.SetVisibleCategories(player, [0]);
        });

        _commandService.AddCommandHandler("changecategories1", (player, args) =>
        {
            _mapNamesService.SetVisibleCategories(player, [1]);
        });

        var permId = _mapNamesService.AddName(new MapName("permanent", Color.White, Vector3.Zero));
        _commandService.AddCommandHandler("mapnamerename", (player, args) =>
        {
            _mapNamesService.SetName(permId, args.ReadAllAsString());
        });
        
        _commandService.AddAsyncCommandHandler("trytakemoneyasync", async (player, args, token) =>
        {
            await player.Money.TryTakeAsync(10, async () =>
            {
                await Task.Delay(1);
                return true;
            }, true, token);
            
            await player.Money.TryTakeAsync(10, async () =>
            {
                return true;
            }, true, token);

            await player.Money.TryTakeAsync(10, async () =>
            {
                return false;
            }, true, token);

            await player.Money.TryTakeAsync(10, async () =>
            {
                throw new Exception();
            }, true, token);

            _chatBox.OutputTo(player, "took 10 money");
        });
        
        _commandService.AddCommandHandler("focusablevehicle", (player, args) =>
        {
            var veh = new FocusableRealmVehicle(_serviceProvider, 404, player.Position);
            _elementFactory.AssociateWithServer(veh);
            _elementFactory.RelayCreated(veh);
        });
        
        _commandService.AddCommandHandler("createcolsphere", (player, args) =>
        {
            var sphere = _elementFactory.CreateCollisionSphere(player.Position, 5);

            void handleEntered(CollisionShape sender, CollisionShapeHitEventArgs e)
            {
                if(e.Element == player)
                {
                    _chatBox.OutputTo(player, "entered");
                }
            }

            void handleLeft(CollisionShape sender, CollisionShapeLeftEventArgs e)
            {
                if (e.Element == player)
                {
                    _chatBox.OutputTo(player, "left");
                }
            }

            sphere.ElementEntered += handleEntered;
            sphere.ElementLeft += handleLeft;
        });

        _commandService.AddCommandHandler("activefuelcontainer", (player, args) =>
        {
            var active = player.Vehicle?.Fuel.Active?.FuelType;
            _chatBox.OutputTo(player, $"Vehicle id: {player.Vehicle?.VehicleId}");
            _chatBox.OutputTo(player, $"Active container: {active}");
        });

        _commandService.AddAsyncCommandHandler("discordtest", async (player, args, token) =>
        {
            if (_discordService == null || _discordService.SendMessage == null)
                return;
            await _discordService.SendMessage(1135218612764934224, "test", token);
        });


        _commandService.AddAsyncCommandHandler("saveall", async (player, args, token) =>
        {
            var start = Stopwatch.GetTimestamp();
            int i = 0;
            foreach (var element in _elementCollection.GetAll().ToList())
            {
                if (element is RealmVehicle vehicle)
                {
                    await vehicle.GetRequiredService<IElementSaveService>().Save(token);
                    i++;
                }
                else if (element is RealmPlayer plr)
                {
                    if (plr.User.IsLoggedIn)
                    {
                        await plr.GetRequiredService<IElementSaveService>().Save(token);
                        i++;
                    }
                }
            }

            var t = ((start - Stopwatch.GetTimestamp()) / Stopwatch.Frequency) * 1000;
            _chatBox.OutputTo(player, $"Saved {i} elements in {t} ms");
        });

        _commandService.AddAsyncCommandHandler("invoketest", async (player, args, token) =>
        {
            try
            {
                await player.Invoke(() =>
                {
                    _chatBox.OutputTo(player, "Ok");
                    return Task.CompletedTask;
                }, token);
            }
            catch(RateLimitRejectedException)
            {
                _chatBox.OutputTo(player, "Rate limited");
            }
        });
        
        _commandService.AddAsyncCommandHandler("invoketesttimeout", async (player, args, token) =>
        {
            try
            {
                await player.Invoke(async () =>
                {
                    await Task.Delay(15000);
                }, token);
            }
            catch (Exception ex)
            {
                _chatBox.OutputTo(player, ex.ToString());
            }
        });

        AddInventoryCommands();
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void AddInventoryCommands()
    {
        _commandService.AddCommandHandler("additem1", (player, args) =>
        {
            if (player.Inventory.TryGetPrimary(out var inventory))
            {
                inventory.AddItem(_itemsCollection, 1);
                _chatBox.OutputTo(player, $"Test item added");
            }
        });

        _commandService.AddCommandHandler("additem2", (player, args) =>
        {
            if (player.Inventory.TryGetPrimary(out var inventory))
            {
                inventory.AddItem(_itemsCollection, 2);
                _chatBox.OutputTo(player, "Test item added");
            }
            else
            {
                _chatBox.OutputTo(player, "Failed to add item");

            }
        });

        _commandService.AddCommandHandler("givedefaultinventory", (player, args) =>
        {
            player.Inventory.CreatePrimary(20);
            _chatBox.OutputTo(player, "Inventory created");
        });

        _commandService.AddCommandHandler("listitems", (player, args) =>
        {
            _chatBox.OutputTo(player, $"Nuumber: {player.Inventory.Primary!.Number}");
            foreach (var item in player.Inventory.Primary!.Items)
            {
                _chatBox.OutputTo(player, $"Item: {item.ItemId}");
            }
        });

        _commandService.AddCommandHandler("removeitem1", (player, args) =>
        {
            player.Inventory.Primary!.RemoveItem(1);
            _chatBox.OutputTo(player, "Item removed");
        });
    }
}

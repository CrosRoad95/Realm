﻿using RealmCore.Resources.Base;
using RealmCore.Server.Modules.Vehicles.Access;
using SlipeServer.Resources.BoneAttach;
using Color = System.Drawing.Color;

namespace RealmCore.BlazorGui.Logic;

enum SampleEnum
{
    Value1,
    Value2,
    Value3,
}

internal sealed class CommandsHostedService : IHostedService
{
    private readonly RealmCommandService _commandService;
    private readonly IElementFactory _elementFactory;
    private readonly ItemsCollection _itemsCollection;
    private readonly ChatBox _chatBox;
    private readonly ILogger<CommandsHostedService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly INametagsService _nametagsService;
    private readonly UsersService _usersService;
    private readonly VehiclesService _vehiclesService;
    private readonly IElementOutlineService _elementOutlineService;
    private readonly IOverlayService _overlayService;
    private readonly IMapNamesService _mapNamesService;
    private readonly VehiclesInUse _vehiclesInUse;
    private readonly IServiceProvider _serviceProvider;
    private readonly IElementCollection _elementCollection;
    private readonly WorldNodesService _worldNodesService;
    private readonly GroupsService _groupsService;
    private readonly MapsService _mapsService;
    private readonly MtaServer _mtaServer;
    private readonly WorldHudService _worldHudService;
    private readonly BoneAttachService _boneAttachService;

    public CommandsHostedService(RealmCommandService commandService, IElementFactory elementFactory,
        ItemsCollection itemsCollection, ChatBox chatBox, ILogger<CommandsHostedService> logger,
        IDateTimeProvider dateTimeProvider, INametagsService nametagsService, UsersService usersService, VehiclesService vehiclesService,
        GameWorld gameWorld, IElementOutlineService elementOutlineService, IAssetsService assetsService, SpawnMarkersService spawnMarkersService, IOverlayService overlayService, AssetsCollection assetsCollection, VehicleUpgradesCollection vehicleUpgradeCollection, VehicleEnginesCollection vehicleEnginesCollection, IMapNamesService mapNamesService, VehiclesInUse vehiclesInUse, IServiceProvider serviceProvider, IElementCollection elementCollection, IDebounceFactory debounceFactory, WorldNodesService worldNodesService, GroupsService groupsService, MapsService mapsService, MtaServer mtaServer, WorldHudService worldHudService, BoneAttachService boneAttachService)
    {
        _commandService = commandService;
        _elementFactory = elementFactory;
        _itemsCollection = itemsCollection;
        _chatBox = chatBox;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _nametagsService = nametagsService;
        _usersService = usersService;
        _vehiclesService = vehiclesService;
        _elementOutlineService = elementOutlineService;
        _overlayService = overlayService;
        _mapNamesService = mapNamesService;
        _vehiclesInUse = vehiclesInUse;
        _serviceProvider = serviceProvider;
        _elementCollection = elementCollection;
        _worldNodesService = worldNodesService;
        _groupsService = groupsService;
        _mapsService = mapsService;
        _mtaServer = mtaServer;
        _worldHudService = worldHudService;
        _boneAttachService = boneAttachService;
        var debounce = debounceFactory.Create(500);
        var debounceCounter = 0;

        _commandService.Add("now", () =>
        {
            _chatBox.Output(_dateTimeProvider.Now.ToString());
        });
        
        _commandService.Add("testpolicy", () =>
        {
            _chatBox.Output("Ok");
        }, ["Admin"]);

        _commandService.Add("testpolicy2", ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, $"authorized: {player.User.HasAuthorizedPolicies(["Admin"])}");
        });

        _commandService.Add("cmdbasic", async ([CallingPlayer] RealmPlayer player, [Range(1, 20)] int a, int b) =>
        {
            await Task.Delay(100);
            _chatBox.Output($"foo: {a}, {b}");
        });

        _commandService.Add("cmdplr", (RealmPlayer player, CancellationToken cancellationToken, int x) =>
        {
            _chatBox.Output($"foo: {player.Name} {cancellationToken} x={x}");
        });

        _commandService.Add("defaultarg", (int x = 10) =>
        {
            _chatBox.Output($"x={x}");
        });
        
        _commandService.Add("enum", (SampleEnum sampleEnum) =>
        {
            _chatBox.Output($"sampleEnum={sampleEnum}");
        });

        _commandService.Add("debounce", async ([CallingPlayer] RealmPlayer player) =>
        {
            debounceCounter++;
            await debounce.InvokeAsync(() =>
            {
                _chatBox.OutputTo(player, $"Counter={debounceCounter}, {DateTime.Now}");
            });
        });

        _commandService.Add("fadecamera", async ([CallingPlayer] RealmPlayer player) =>
        {
            await player.FadeCameraAsync(CameraFade.Out, 5);
            await player.FadeCameraAsync(CameraFade.In, 5);
        });

        _commandService.Add("fadecamera2", async ([CallingPlayer] RealmPlayer player) =>
        {
            var cancelationTokenSource = new CancellationTokenSource();
            cancelationTokenSource.CancelAfter(2000);
            await player.FadeCameraAsync(CameraFade.Out, 5, cancelationTokenSource.Token);
            await player.FadeCameraAsync(CameraFade.In, 5);
        });

        #region Commands for focusable tests
        _commandService.Add("focusable", ([CallingPlayer] RealmPlayer player) =>
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

        _commandService.Add("outline1", async ([CallingPlayer] RealmPlayer player) =>
        {
            var worldObject = _elementFactory.CreateObject(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), ObjectModel.Gunbox);
            _elementOutlineService.SetElementOutline(worldObject, Color.Red);
            _chatBox.OutputTo(player, "Created outline");
            await Task.Delay(2000);
            _elementOutlineService.RemoveElementOutline(worldObject);
            _chatBox.OutputTo(player, "Destroyed outline");
        });

        _commandService.Add("outline2", async ([CallingPlayer] RealmPlayer player) =>
        {
            var worldObject = _elementFactory.CreateObject(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), ObjectModel.Gunbox);
            _elementOutlineService.SetElementOutline(worldObject, Color.Red);
            _chatBox.OutputTo(player, "Created outline");
            await Task.Delay(2000);
            worldObject.Destroy();
            _chatBox.OutputTo(player, "Destroyed player");
        });
        #endregion

        _commandService.Add("playtime", ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, $"playtime: {player.PlayTime.PlayTime}, total play time: {player.PlayTime.TotalPlayTime}");
        });

        _commandService.Add("givemoney", ([CallingPlayer] RealmPlayer player, decimal amount) =>
        {
            player.Money.Give(amount);
            _chatBox.OutputTo(player, $"give money: {amount}, total money: {player.Money.Amount}");
        });

        _commandService.Add("takemoney", ([CallingPlayer] RealmPlayer player, decimal amount) =>
        {
            player.Money.Take(amount);
            _chatBox.OutputTo(player, $"take money: {amount}, total money: {player.Money.Amount}");
        });

        _commandService.Add("setmoney", ([CallingPlayer] RealmPlayer player, decimal amount) =>
        {
            player.Money.Amount = amount;
            _chatBox.OutputTo(player, $"set money: {amount}, total money: {player.Money.Amount}");
        });

        _commandService.Add("money", ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, $"total money: {player.Money.Amount}");
        });

        _commandService.Add("cvwithlicenserequired", ([CallingPlayer] RealmPlayer player, VehicleModel vehicleModel) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), vehicleModel);
            vehicle.AccessController = new VehicleLicenseRequirementAccessController(10);
        });

        _commandService.Add("givelicense10", ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.Licenses.TryAdd(10))
                _chatBox.OutputTo(player, $"License 10 added");
        });

        _commandService.Add("cv", ([CallingPlayer] RealmPlayer player, ushort vehicleModel) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)vehicleModel);
            vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
            vehicle.PartDamage.TryAddPart(1, 1337);
            vehicle.AccessController = new VehicleExclusiveAccessController(player);
            _chatBox.OutputTo(player, $"veh created");
        });

        _commandService.Add("cvprivate", async ([CallingPlayer] RealmPlayer player) =>
        {
            var vehicle = await _vehiclesService.CreatePersistantVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)404);
            if (vehicle == null)
                return;

            vehicle.Upgrades.AddUpgrade(1);
            vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
            vehicle.PartDamage.TryAddPart(1, 1337);
            vehicle.Access.TryAddAsOwner(player);
            _chatBox.OutputTo(player, $"Stworzono pojazd o id: {vehicle.VehicleId}");
        });

        _commandService.Add("cvveh", ([CallingPlayer] RealmPlayer player) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)404);
            vehicle.Upgrades.AddUpgrade(1);
            vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
            vehicle.PartDamage.TryAddPart(1, 1337);
            vehicle.Access.TryAddAsOwner(player);
            vehicle.AddPassenger(0, player);
            _chatBox.OutputTo(player, "Stworzono pojazd.");
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                while (player.Vehicle != null)
                {
                    await Task.Delay(2000);

                    _chatBox.OutputTo(player, $"Paliwo {vehicle.Fuel.Active!.Amount}");
                }
            });
        });

        _commandService.Add("converttopersistent", async ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.Vehicle == null)
            {
                _chatBox.OutputTo(player, "Wejdź do pojazdu.");
                return;
            }
            var vehicle = await _vehiclesService.ConvertToPersistantVehicle(player.Vehicle);
            if (vehicle == null)
                return;

            vehicle.Access.TryAddAsOwner(player);
            _chatBox.OutputTo(player, $"Skonwertowano pojazd, id: {vehicle.VehicleId}");
        });

        _commandService.Add("spawnvehhere", async ([CallingPlayer] RealmPlayer player, int vehicleId) =>
        {
            var vehicleLoader = player.GetRequiredService<VehiclesLoader>();
            var vehicle = await vehicleLoader.LoadVehicleById(vehicleId);
            var location = player.GetLocation(player.GetPointFromDistanceRotationOffset(3));
            vehicle.SetLocation(location);
            _chatBox.OutputTo(player, $"Załadowano pojazd na pozycji: {location}");
        });

        _commandService.Add("spawnveh", async ([CallingPlayer] RealmPlayer player, int vehicleId) =>
        {
            var vehicleLoader = player.GetRequiredService<VehiclesLoader>();
            var vehicle = await vehicleLoader.LoadVehicleById(vehicleId);
            _chatBox.OutputTo(player, $"Załadowano pojazd na pozycji: {vehicle.GetLocation()}");
        });

        _commandService.Add("despawn", async ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.Vehicle == null)
            {
                _chatBox.OutputTo(player, "Enter vehicle.");
                return;
            }
            await _vehiclesService.Destroy(player.Vehicle);
        });
        
        _commandService.Add("respawnvehicle", async ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.FocusedElement is not RealmVehicle vehicle)
            {
                _chatBox.OutputTo(player, "You are not focused on vehicle.");
                return;
            }

            var vehicleLoader = player.GetRequiredService<VehiclesLoader>();
            var vehicleId = vehicle.Persistence.Id;
            await _vehiclesService.Destroy(vehicle);
            await vehicleLoader.LoadVehicleById(vehicleId);
            _chatBox.OutputTo(player, $"Vehicle of id {vehicleId} respawned.");
        });

        _commandService.Add("vehiclesinuse", ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, $"Pojazdy w użyciu: {string.Join(", ", _vehiclesInUse.ActiveVehiclesIds)}");
        });

        _commandService.Add("gp", ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, player.GetLocation().ToString());
        });

        _commandService.Add("myveh", ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, player.Vehicle?.ToString() ?? "brak");
        });

        _commandService.Add("savemyveh", async ([CallingPlayer] RealmPlayer player) =>
        {
            var vehicle = player.Vehicle ?? throw new InvalidOperationException();
            await vehicle.GetRequiredService<ElementSaveService>().Save();
        });

        _commandService.Add("exclusivecv", ([CallingPlayer] RealmPlayer player) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)404);
            vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
            vehicle.AccessController = new VehicleExclusiveAccessController(player);
        });

        _commandService.Add("noaccesscv", ([CallingPlayer] RealmPlayer player) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)404);
            vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
            vehicle.AccessController = VehicleNoAccessController.Instance;
        });

        _commandService.Add("testachievement", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Achievements.UpdateProgress(1, 2, 10);
            _chatBox.OutputTo(player, $"progressed achieviement 'test'");
        });

        _commandService.Add("addupgrade", ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.JobUpgrades.TryAdd(1, 1))
                _chatBox.OutputTo(player, "Upgrade added");
            else
                _chatBox.OutputTo(player, "Failed to add upgrade");
        });

        _commandService.Add("addvehicleupgrade", ([CallingPlayer] RealmPlayer player) =>
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

        _commandService.Add("addtestdata", async ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.JobUpgrades.TryAdd(1, 1))
                _chatBox.OutputTo(player, "Upgrade added");
            else
                _chatBox.OutputTo(player, "Failed to add upgrade");

            if (player.Inventory.TryGetPrimary(out var inventory))
            {
                using(var access = inventory.Open())
                {
                    access.TryAddItem(1);
                }
                _chatBox.OutputTo(player, $"Test item added");
            }

            if (player.Licenses.TryAdd(1))
                _chatBox.OutputTo(player, $"Test license added: 'test123' of id 1");

            player.Money.Amount = (decimal)Random.Shared.NextDouble() * 1000;
            _chatBox.OutputTo(player, $"Set money to: {player.Money}");


            player.Achievements.UpdateProgress(1, 2, 10);
            _chatBox.OutputTo(player, $"Updated achievement 'test' progress to 2");

            {
                var vehicle = await _vehiclesService.CreatePersistantVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation), (VehicleModel)404);
                if (vehicle == null)
                    return;

                vehicle.Upgrades.AddUpgrade(1);
                vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
                vehicle.PartDamage.TryAddPart(1, 1337);
            }
        });

        _commandService.Add("giveexperience", ([CallingPlayer] RealmPlayer player, uint amount) =>
        {
            player.Level.GiveExperience(amount);
            _chatBox.OutputTo(player, $"gave experience: {amount}, level: {player.Level.Current}, experience: {player.Level.Experience}");
        });

        _commandService.Add("cvforsale", ([CallingPlayer] RealmPlayer player) =>
        {
            _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), Vector3.Zero), (VehicleModel)404);
        });

        _commandService.Add("privateoutlinetest", async ([CallingPlayer] RealmPlayer player) =>
        {
            var @object = _elementFactory.CreateObject(new Location(player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero), ObjectModel.Gunbox);
            await Task.Delay(2000);
            elementOutlineService.SetElementOutlineForPlayer(player, @object, Color.Red);
            await Task.Delay(1000);
            elementOutlineService.RemoveElementOutlineForPlayer(player, @object);
            _chatBox.OutputTo(player, "removed");
        });

        _commandService.Add("spawnbox", ([CallingPlayer] RealmPlayer player) =>
        {
            void handleCollisionEnabledChanged(Element sender, ElementChangedEventArgs<bool> args)
            {
                ;
            }

            var worldObject = _elementFactory.CreateFocusableObject(new Location(player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero), ObjectModel.Gunbox);
            worldObject.AreCollisionsEnabled = false;
            worldObject.CollisionEnabledChanged += handleCollisionEnabledChanged;
            worldObject.Interaction = new LiftableInteraction();
            _chatBox.OutputTo(player, "spawned box");
        });
        
        _commandService.Add("spawnbox2", async ([CallingPlayer] RealmPlayer player) =>
        {
            var bin = new WorldObject(1339, player.Position).AssociateWith(mtaServer);
            bin.AreCollisionsEnabled = false;
            _boneAttachService.Attach(bin, player, BoneId.Spine1, Vector3.Zero);
            await Task.Delay(1000);
            _boneAttachService.Detach(bin);
        });

        _commandService.Add("durationinteractionbox", ([CallingPlayer] RealmPlayer player) =>
        {
            var worldObject = _elementFactory.CreateFocusableObject(new Location(player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero), ObjectModel.Gunbox);
            var interaction = new DurationBasedHoldWithRingEffectInteraction(overlayService);

            void handleInteractionCompleted(DurationBasedHoldInteraction durationBasedHoldInteractionComponent, RealmPlayer player, bool succeed)
            {
                _chatBox.OutputTo(player, $"Succeed: {succeed}");
            }

            interaction.InteractionCompleted += handleInteractionCompleted;
            worldObject.Interaction = interaction;
        });
        
        _commandService.Add("spawnscopedbox", ([CallingPlayer] RealmPlayer player) =>
        {
            var worldObject = player.ElementFactory.CreateFocusableObject(new Location(player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero), ObjectModel.Gunbox);
            worldObject.Interaction = new LiftableInteraction();
        });

        _commandService.Add("spawnboxfront", ([CallingPlayer] RealmPlayer player) =>
        {
            var front = player.GetPointFromDistanceRotation(2);
            var worldObject = _elementFactory.CreateObject(new Location(front), ObjectModel.Gunbox);
            worldObject.LookAt(player);
        });

        _commandService.Add("spawnmybox", ([CallingPlayer] RealmPlayer player) =>
        {
            var worldObject = _elementFactory.CreateObject(new Location(player.Position + new Vector3(4, 0, -0.65f), Vector3.Zero), ObjectModel.Gunbox);
            worldObject.Interaction = new LiftableInteraction();
            worldObject.TrySetOwner(player);
        });

        _commandService.Add("spawncolshapeforme", ([CallingPlayer] RealmPlayer player) =>
        {
            var marker = player.ElementFactory.CreateMarker(new Location(player.Position + new Vector3(4, 0, 0), Vector3.Zero), MarkerType.Arrow, 2, Color.Red);
            marker.CollisionShape.ElementEntered += (that, enteredElement) =>
            {
                _chatBox.Output($"entered2 {enteredElement}");
            };
        });

        _commandService.Add("animation", async ([CallingPlayer] RealmPlayer player, string animationName) =>
        {
            if (Enum.TryParse<Animation>(animationName, out var animation))
            {
                try
                {
                    _chatBox.OutputTo(player, $"Started animation '{animationName}'");
                    //player.SetAnimation("CARRY", "crry_prtial", TimeSpan.FromMilliseconds(100), true, false);
                    await player.DoAnimationAsync(animation);
                    _chatBox.OutputTo(player, $"Finished animation '{animationName}'");
                }
                catch (NotSupportedException)
                {
                    _chatBox.OutputTo(player, $"Animation '{animationName}' is not supported");
                }
            }
            else
                _chatBox.OutputTo(player, $"Animation '{animationName}' not found.");
        });

        _commandService.Add("createtestpickups", ([CallingPlayer] RealmPlayer player) =>
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

        _commandService.Add("interior", ([CallingPlayer] RealmPlayer player, byte interior) =>
        {
            player.Interior = interior;
        });

        _commandService.Add("dimension", ([CallingPlayer] RealmPlayer player, ushort dimension) =>
        {
            player.Dimension = dimension;
        });

        _commandService.Add("devtools", ([CallingPlayer] RealmPlayer player) =>
        {
            var browser = player.Browser;
            player.Admin.DevelopmentMode = true;
            browser.DevTools = !browser.DevTools;
            _chatBox.OutputTo(player, $"Devtools {browser.DevTools}");
        }, null);

        _commandService.Add("guitest1", ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.Gui.Current == null)
                player.Gui.Current = new Counter1Gui(player);
            else
                player.Gui.Current = null;
        });

        commandService.Add("browserloadcounter1", ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.Browser.Visible)
            {
                player.Browser.Visible = false;
                return;
            }
            player.Browser.Open("/realmUi/counter1");
            _chatBox.OutputTo(player, "Loaded counter 1");
        });

        _commandService.Add("logout", async ([CallingPlayer] RealmPlayer player) =>
        {
            await _usersService.LogOut(player);
        });

        _commandService.Add("scopedelements", async ([CallingPlayer] RealmPlayer player) =>
        {
            using var scope = player.GetRequiredService<IScopedElementFactory>().CreateScope();
            scope.CreateObject(new Location(player.Position + new Vector3(3, 0, 0), Vector3.Zero), (ObjectModel)1337);
            await Task.Delay(1000);
        });

        _commandService.Add("createelementsforme", ([CallingPlayer] RealmPlayer player) =>
        {
            player.ElementFactory.CreateObject(new Location(player.Position + new Vector3(3, 0, 0), Vector3.Zero), (ObjectModel)1337);
        });

        _commandService.Add("listevents", ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, "Events:");
            foreach (var item in player.Events)
            {
                _chatBox.OutputTo(player, $"Event: {item.Id} - {item.EventType}: {item.Metadata}");
            }
        });
        _commandService.Add("addevent", ([CallingPlayer] RealmPlayer player, [ReadRestAsString] string text) =>
        {
            player.Events.Add(1, text);
            _chatBox.OutputTo(player, "Added");
        });

        _commandService.Add("fetchmoreevents", async ([CallingPlayer] RealmPlayer player) =>
        {
            var fetched = await player.Events.FetchMore(10);
            _chatBox.OutputTo(player, $"fetched {fetched.Length} more events");
        });

        _commandService.Add("wait", async ([CallingPlayer] RealmPlayer player) =>
        {
            await Task.Delay(5000);
        });

        _commandService.Add("asyncFadeCamera", async ([CallingPlayer] RealmPlayer player) =>
        {
            await using (await player.FadeCameraAsync(CameraFade.Out, 0.5f))
            {
                await Task.Delay(2000);
            }
        });

        _commandService.Add("warppedintovehicle", async ([CallingPlayer] RealmPlayer player) =>
        {
            var vehicle = _elementFactory.CreateVehicle(new Location(player.Position + new Vector3(4, 0, 0), player.Rotation, Interior: 4, Dimension: 3), VehicleModel.Perennial);

            player.WarpIntoVehicle(vehicle);
        });

        //_commandService.Add("searchplayers", ([CallingPlayer] RealmPlayer player) =>
        //{
        //    var foundPlayers = args.SearchPlayers();
        //    foreach (var foundPlayer in foundPlayers)
        //    {
        //        _chatBox.OutputTo(player, $"Found player: {foundPlayer.Name}");
        //    }
        //});

        //_commandService.Add("searchVehicles", ([CallingPlayer] RealmPlayer player) =>
        //{
        //    var foundVehicles = args.SearchVehicles();
        //    foreach (var foundVehicle in foundVehicles)
        //    {
        //        _chatBox.OutputTo(player, $"Found vehicle: {foundVehicle.Name}");
        //    }
        //});

        //_commandService.Add("select", ([CallingPlayer] RealmPlayer player) =>
        //{
        //    var pattern = args.ReadArgument();
        //    var foundPlayers = args.SearchPlayers(pattern, PlayerSearchOption.All | PlayerSearchOption.AllowEmpty);
        //    var foundVehicles = args.SearchVehicles(pattern);
        //    int a = 0;
        //    foreach (var foundPlayer in foundPlayers)
        //    {
        //        if (player.SelectedElements.TryAdd(foundPlayer))
        //            a++;
        //    }
        //    foreach (var foundVehicle in foundVehicles)
        //    {
        //        if (player.SelectedElements.TryAdd(foundVehicle))
        //            a++;
        //    }
        //    _chatBox.OutputTo(player, $"Selected {a} elements.");
        //});

        //_commandService.Add("deselect", ([CallingPlayer] RealmPlayer player) =>
        //{
        //    var pattern = args.ReadArgument();
        //    var foundPlayers = args.SearchPlayers(pattern, PlayerSearchOption.All | PlayerSearchOption.AllowEmpty);
        //    var foundVehicles = args.SearchVehicles(pattern);
        //    int a = 0;
        //    foreach (var foundPlayer in foundPlayers)
        //    {
        //        if (player.SelectedElements.TryRemove(foundPlayer))
        //            a++;
        //    }
        //    foreach (var foundVehicle in foundVehicles)
        //    {
        //        if (player.SelectedElements.TryRemove(foundVehicle))
        //            a++;
        //    }
        //    _chatBox.OutputTo(player, $"Deselected {a} elements.");
        //});

        _commandService.Add("listselected", ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, "Selected elements:");
            foreach (var element in player.SelectedElements)
            {
                _chatBox.OutputTo(player, $"Selected element: {element}");
            }
        });

        _commandService.Add("setcolor", ([CallingPlayer] RealmPlayer player, KnownColor color) =>
        {
            foreach (var element in player.SelectedElements)
            {
                if (element is RealmVehicle vehicle)
                {
                    vehicle.Colors.Primary = Color.FromKnownColor(color);
                }
            }
        });

        _commandService.Add("setbind", ([CallingPlayer] RealmPlayer player) =>
        {
            player.SetBind("p", (player, keyState) =>
            {
                if (keyState == KeyState.Down)
                {
                    foreach (var element in player.SelectedElements)
                        _elementOutlineService.SetElementOutline(element, Color.Green);
                }
                if (keyState == KeyState.Up)
                {
                    foreach (var element in player.SelectedElements)
                        _elementOutlineService.RemoveElementOutline(element);
                }
            });
        });

        _commandService.Add("scheduler1", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Scheduler.ScheduleJobOnce(() =>
            {
                _chatBox.OutputTo(player, "Once");
                Console.WriteLine("Player once");
                return Task.CompletedTask;
            }, TimeSpan.FromSeconds(5));
        });

        _commandService.Add("scheduler2", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Scheduler.ScheduleJob(() =>
            {
                _chatBox.OutputTo(player, "repeat");
                Console.WriteLine("Player repeat");
                return Task.CompletedTask;
            }, TimeSpan.FromSeconds(2));
        });

        _commandService.Add("setstat", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Statistics.SetGtaSa(PedStat.BIKE_SKILL, 999);
            player.Statistics.SetGtaSa(PedStat.CYCLE_SKILL, 999);
        });

        _commandService.Add("playtimecategory", ([CallingPlayer] RealmPlayer player, int category) =>
        {
            player.PlayTime.Category = category;
            _chatBox.OutputTo(player, $"Category changed to {player.PlayTime.Category}");
        });

        _commandService.Add("playtimelist", ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, $"Play time list:");
            foreach (var item in player.PlayTime)
            {
                _chatBox.OutputTo(player, $"{item.Category} = {item.PlayTime}");
            }
        });

        _commandService.Add("toggleAllControlsScope", async ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, $"start {player.Controls.ForwardsEnabled}");
            {
                using var _ = new ToggleControlsScope(player);

                await Task.Delay(2500);
            }

            _chatBox.OutputTo(player, $"done {player.Controls.ForwardsEnabled}");
        });

        _commandService.Add("mapnameadd", ([CallingPlayer] RealmPlayer player) =>
        {
            var id1 = _mapNamesService.AddNameFor(new MapName("test1", Color.White, new Vector3(1000, 0, 0)), player);
            var id2 = _mapNamesService.AddNameFor(new MapName("test2", Color.Red, new Vector3(1000, 500, 0), Category: 1), player);
            var id3 = _mapNamesService.AddNameFor(new MapName("destroyed", Color.Red, new Vector3(1000, 1000, 0), Category: 1), player);
            _mapNamesService.RemoveFor(id3, player);
        });

        _commandService.Add("mapnameaddtemp", async ([CallingPlayer] RealmPlayer player) =>
        {
            var id = _mapNamesService.AddName(new MapName("temp name", Color.Yellow, new Vector3(-1000, 0, 0)));
            await Task.Delay(3000);
            _mapNamesService.Remove(id);
        });

        _commandService.Add("mapnameaddtemprename", async ([CallingPlayer] RealmPlayer player) =>
        {
            var id = _mapNamesService.AddName(new MapName("temp name", Color.Yellow, new Vector3(-1000, 0, 0)));
            for (int i = 0; i < 10; i++)
            {
                _mapNamesService.SetNameFor(id, $"new name {i}", player);
                await Task.Delay(1000);
            }
            _mapNamesService.Remove(id);
        });

        _commandService.Add("changecategories0", ([CallingPlayer] RealmPlayer player) =>
        {
            _mapNamesService.SetVisibleCategories(player, [0]);
        });

        _commandService.Add("changecategories1", ([CallingPlayer] RealmPlayer player) =>
        {
            _mapNamesService.SetVisibleCategories(player, [1]);
        });

        var permId = _mapNamesService.AddName(new MapName("permanent", Color.White, Vector3.Zero));
        _commandService.Add("mapnamerename", ([CallingPlayer] RealmPlayer player, [ReadRestAsString] string text) =>
        {
            _mapNamesService.SetName(permId, text);
        });

        _commandService.Add("trytakemoneyasync", async ([CallingPlayer] RealmPlayer player) =>
        {
            await player.Money.TryTakeAsync(10, async () =>
            {
                await Task.Delay(1);
                return true;
            }, true);

            await player.Money.TryTakeAsync(10, async () =>
            {
                return true;
            }, true);

            await player.Money.TryTakeAsync(10, async () =>
            {
                return false;
            }, true);

            await player.Money.TryTakeAsync(10, async () =>
            {
                throw new Exception();
            }, true);

            _chatBox.OutputTo(player, "took 10 money");
        });

        _commandService.Add("createcolsphere", ([CallingPlayer] RealmPlayer player) =>
        {
            var sphere = _elementFactory.CreateCollisionSphere(player.Position, 5);

            void handleEntered(CollisionShape sender, CollisionShapeHitEventArgs e)
            {
                if (e.Element == player)
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

        _commandService.Add("activefuelcontainer", ([CallingPlayer] RealmPlayer player) =>
        {
            var fuelContainer = player.Vehicle?.Fuel.Active!;
            var active = fuelContainer.FuelType;
            _chatBox.OutputTo(player, $"Vehicle id: {player.Vehicle?.VehicleId}");
            _chatBox.OutputTo(player, $"Active container: {active}");
            _chatBox.OutputTo(player, $"Fuel: {fuelContainer.Amount}/{fuelContainer.MaxCapacity}");
        });

        _commandService.Add("saveall", async ([CallingPlayer] RealmPlayer player) =>
        {
            var start = Stopwatch.GetTimestamp();
            int i = 0;
            foreach (var element in _elementCollection.GetAll().ToList())
            {
                if (element is RealmVehicle vehicle)
                {
                    await vehicle.GetRequiredService<ElementSaveService>().Save();
                    i++;
                }
                else if (element is RealmPlayer plr)
                {
                    if (plr.User.IsLoggedIn)
                    {
                        await plr.GetRequiredService<ElementSaveService>().Save();
                        i++;
                    }
                }
            }

            var t = (start - Stopwatch.GetTimestamp()) / Stopwatch.Frequency * 1000;
            _chatBox.OutputTo(player, $"Saved {i} elements in {t} ms");
        });

        _commandService.Add("invoketest", async ([CallingPlayer] RealmPlayer player) =>
        {
            try
            {
                await player.Invoke(() =>
                {
                    _chatBox.OutputTo(player, "Ok");
                    return Task.CompletedTask;
                });
            }
            catch (RateLimitRejectedException)
            {
                _chatBox.OutputTo(player, "Rate limited");
            }
        });

        _commandService.Add("invoketesttimeout", async ([CallingPlayer] RealmPlayer player) =>
        {
            try
            {
                await player.Invoke(async () =>
                {
                    await Task.Delay(15000);
                });
            }
            catch (Exception ex)
            {
                _chatBox.OutputTo(player, ex.ToString());
            }
        });

        _commandService.Add("kickme", ([CallingPlayer] RealmPlayer player) =>
        {
            void Player_Disconnected(Player sender, PlayerQuitEventArgs e)
            {
                Console.WriteLine("Kicked");
            }

            player.Disconnected += Player_Disconnected;
            player.Kick();
        });

        _commandService.Add("test123", ([CallingPlayer] RealmPlayer player) =>
        {
            ;
        });

        _commandService.Add("startsession", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Sessions.Begin<TestSession>();
            player.Sessions.Ended += (PlayerSessionsFeature sessions, Session session) =>
            {
                player.Money.Give(100);
            };
            _chatBox.OutputTo(player, "Started");
        });
        
        _commandService.Add("currentInteractElement", ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, player.CurrentInteractElement?.ToString() ?? "<none>");
        });
        
        _commandService.Add("dailyTaskProgress", ([CallingPlayer] RealmPlayer player) =>
        {
            player.DailyTasks.TryBeginDailyTask(1);
            player.DailyTasks.TryDoProgress(1, 10);
            _chatBox.OutputTo(player, $"Daily task progressed to: {player.DailyTasks.GetProgress(1)}");
        });

        _commandService.Add("resendAllNametags", ([CallingPlayer] RealmPlayer player) =>
        {
            nametagsService.ResendAllNametagsToPlayer(player);
        });

        _commandService.Add("togglemynametag", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Nametag.ShowMyNametag = !player.Nametag.ShowMyNametag;
        });
        
        _commandService.Add("setnametag", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Nametag.Text = Guid.NewGuid().ToString();
        });
        
        _commandService.Add("listassets", ([CallingPlayer] RealmPlayer player) =>
        {
            foreach (var pair in assetsCollection.Assets)
            {
                _chatBox.OutputTo(player, $"{pair.Key} - {pair.Value}");
            }
        });
        
        _commandService.Add("replacedmodels", ([CallingPlayer] RealmPlayer player) =>
        {
            foreach (var pair in assetsService.ReplacedModels)
            {
                _chatBox.OutputTo(player, $"Model: {(ushort)pair.Key} - {pair.Value.collisionAsset}");
            }
        });
        
        _commandService.Add("day", ([CallingPlayer] RealmPlayer player) =>
        {
            gameWorld.SetTime(12, 0);
        });

        _commandService.Add("night", ([CallingPlayer] RealmPlayer player) =>
        {
            gameWorld.SetTime(2, 0);
        });
        
        _commandService.Add("testupload", async ([CallingPlayer] RealmPlayer player) =>
        {
            await player.GetRequiredService<UploadedFilesRepository>().Add("sampleNone", "txt", 123, "[]", DateTime.Now);
            await player.GetRequiredService<UploadedFilesRepository>().Add("sampleUser", "txt", 123, "[]", DateTime.Now, player.UserId);
        });
        
        _commandService.Add("setskin", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Model = 23;
        });
        
        _commandService.Add("readstring", ([CallingPlayer] RealmPlayer player, [ReadRestAsString] string str) =>
        {
            _chatBox.OutputTo(player, str);
        });
        
        _commandService.Add("readbool", ([CallingPlayer] RealmPlayer player, bool boolean1, bool boolean2 = true) =>
        {
            _chatBox.OutputTo(player, $"{boolean1}, {boolean2}");
        });
        
        _commandService.Add("serverinfo", ([CallingPlayer] RealmPlayer player) =>
        {
            _logger.LogInformation("Server uptime: {uptime}", _dateTimeProvider.Now - _mtaServer.StartDatetime);
            _logger.LogInformation("Players: {playerCount}, logged in players: {loggedInPlayers}", _elementCollection.GetByType<RealmPlayer>().Count(), _elementCollection.GetByType<RealmPlayer>().Where(x => x.User.IsLoggedIn).Count());
            _logger.LogInformation("Vehicles: {vehiclesCount}", _elementCollection.GetByType<RealmVehicle>());
            _logger.LogInformation("Elements count: {elementsCount}", _elementCollection.Count);
            _logger.LogInformation("Loaded global maps: {loadedMaps}", string.Join(", ", _mapsService.LoadedMaps));
        });
        
        AddBoostCommands();
        AddSecretsCommands();
        AddNodesCommands();
        AddInventoryCommands();
        AddCommandGroups();
        AddHudCommands();
        AddSettingsCommands();
        AddVehicleAccessCommands();
        AddOverlayCommands();
    }

    internal sealed class TestSession : Session
    {
        public override string Name => "Test";
        public int EndedCount { get; private set; }

        public TestSession(PlayerContext playerContext, IDateTimeProvider dateTimeProvider) : base(playerContext.Player, dateTimeProvider)
        {
        }

        protected override void OnEnded()
        {
            EndedCount++;
        }
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void AddNodesCommands()
    {
        _commandService.Add("spawnnode", async ([CallingPlayer] RealmPlayer player) =>
        {
            var node = await _worldNodesService.Create<SampleNode>(new Location(player.Position + new Vector3(4, 0, 0)), new SampleState(1337));
            _chatBox.OutputTo(player, "spawned node");
        });

        _commandService.Add("spawnnode2", async ([CallingPlayer] RealmPlayer player) =>
        {
            var node = await _worldNodesService.Create<SampleNode2>(new Location(player.Position + new Vector3(4, 0, 0)), new SampleState(0));
            _chatBox.OutputTo(player, "spawned node counter");
        });

        _commandService.Add("spawnnode3", async ([CallingPlayer] RealmPlayer player) =>
        {
            var node = await _worldNodesService.Create<SampleNode>(new Location(player.Position + new Vector3(4, 0, 0)), new SampleState(1337));
            _chatBox.OutputTo(player, $"temporarly spawned node id: {node.Id}");
            await Task.Delay(5000);
            await _worldNodesService.Destroy(node);
            _chatBox.OutputTo(player, $"destroyed temporarly spawned node id: {node.Id}");
        });

    }

    private void AddBoostCommands()
    {
        _commandService.Add("boostslist", ([CallingPlayer] RealmPlayer player) =>
        {
            _chatBox.OutputTo(player, "Boosts");
            foreach (var boostId in player.Boosts.AllBoosts)
            {
                _chatBox.OutputTo(player, $"Boost: {boostId}");
            }

            _chatBox.OutputTo(player, "Active boosts");
            foreach (var activeBoost in player.Boosts.ActiveBoosts)
            {
                _chatBox.OutputTo(player, $"Boost: {activeBoost.BoostId} remaining time: {activeBoost.RemainingTime}");
            }
        });

        _commandService.Add("giveboost", ([CallingPlayer] RealmPlayer player, int id) =>
        {
            player.Boosts.AddBoost(id);
            _chatBox.OutputTo(player, "Added");
        });

        _commandService.Add("activateboost", ([CallingPlayer] RealmPlayer player, int id) =>
        {
            var activated = player.Boosts.TryActivateBoost(id, TimeSpan.FromMinutes(5));
            _chatBox.OutputTo(player, $"Boost activated: {activated}");
        });

        _commandService.Add("activateboost2", ([CallingPlayer] RealmPlayer player, int id) =>
        {
            var activated = player.Boosts.TryActivateBoost(id, TimeSpan.FromSeconds(5));
            _chatBox.OutputTo(player, $"Boost activated: {activated}");
        });

        _commandService.Add("isboostactive", ([CallingPlayer] RealmPlayer player, int id) =>
        {
            _chatBox.OutputTo(player, $"Is boost active: {player.Boosts.IsActive(id)}");
        });
    }

    private void AddSecretsCommands()
    {
        _commandService.Add("secretsreveal", ([CallingPlayer] RealmPlayer player, int groupId, int secretId) =>
        {
            var revealed = player.Secrets.TryReveal(groupId, secretId);
            _chatBox.OutputTo(player, $"Result: {revealed}");
        });
        
        _commandService.Add("secretsgetbygroupid", ([CallingPlayer] RealmPlayer player, int groupId) =>
        {
            var secretIds = player.Secrets.GetByGroupId(groupId);
            _chatBox.OutputTo(player, $"Result: {string.Join(", ", secretIds)}");
        });
    }

    private void AddInventoryCommands()
    {
        _commandService.Add("listitems", ([CallingPlayer] RealmPlayer player) =>
        {
            var inventory = player.Inventory.Primary;
            if (inventory == null)
            {
                _chatBox.OutputTo(player, "Failed to list items. You don't have inventory");
                return;
            }

            _chatBox.OutputTo(player, $"Type: {inventory.GetType().Name}");
            _chatBox.OutputTo(player, $"Number: {inventory.Number}");
            using (var access = inventory.Open())
            {
                foreach (var item in access.Items)
                {
                    _chatBox.OutputTo(player, $"Item: {item.ItemId}");
                }
            }
        });

        _commandService.Add("additem", ([CallingPlayer] RealmPlayer player, uint itemId) =>
        {
            if (player.Inventory.TryGetPrimary(out var inventory))
            {
                using (var access = inventory.Open())
                {
                    access.TryAddItem(itemId);
                }
                _chatBox.OutputTo(player, "Test item added");
            }
            else
            {
                _chatBox.OutputTo(player, "Failed to list items. You don't have inventory");

            }
        });

        _commandService.Add("givedefaultinventory", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Inventory.CreatePrimary(20);
            _chatBox.OutputTo(player, "Inventory created");
        });

        _commandService.Add("removeitem1", ([CallingPlayer] RealmPlayer player) =>
        {
            using var access = player.Inventory.Primary!.Open();
            access.RemoveItem(access.FindItem(x => x.ItemId == 1)!);
            _chatBox.OutputTo(player, "Item removed");
        });
    }

    private void AddCommandGroups()
    {
        _commandService.Add("groupcreate", async ([CallingPlayer] RealmPlayer player) =>
        {
            var group = await _groupsService.Create(Guid.NewGuid().ToString());
            await _groupsService.AddMember(player.UserId, group!.Id);
            var role = await _groupsService.CreateRole(group!.Id, "test", []);
            var changed = await _groupsService.SetMemberRole(group.Id, player.UserId, role.Id);
            await _groupsService.SetRolePermissions(role.Id, [1, 3, 5]);
            _chatBox.OutputTo(player, $"Created group {group.Id} with role.");
        });
        
        _commandService.Add("grouplist", async ([CallingPlayer] RealmPlayer player) =>
        {
            var groups = await _groupsService.GetAll(1);
            _chatBox.OutputTo(player, "Groups:");
            foreach (var group in groups)
            {
                _chatBox.OutputTo(player, $"ID: {(int)group.Id}, Name: {group.Name}");
            }
        });
        
        _commandService.Add("listmygroups", ([CallingPlayer] RealmPlayer player) =>
        {
            var groups = player.Groups.Select(x => x.GroupId).ToArray();
            _chatBox.OutputTo(player, $"My groups: {string.Join(", ", groups)}");
            foreach (var group in player.Groups)
            {
               _chatBox.OutputTo(player, $"\tMy group: {group.GroupId}, name: {group.Group?.Name}, role: {(group.RoleId != null ? group.RoleId : "<none>")}");
               _chatBox.OutputTo(player, $"\tPermissions: {(group.Permissions != null ? string.Join(", ", group.Permissions.Select(x => x.ToString())) : "<none>")}");
            }
        });
        
        _commandService.Add("groupsetrole", async ([CallingPlayer] RealmPlayer player, int groupId, int roleId) =>
        {
            var changed = await _groupsService.SetMemberRole(groupId, player.UserId, roleId);
            if(changed)
            {
                _chatBox.OutputTo(player, $"Role in group {groupId} changed to {roleId}");
            }
            else
            {
                _chatBox.OutputTo(player, $"Failed to change role");
            }
        });
        
        _commandService.Add("groupinfo", async ([CallingPlayer] RealmPlayer player, int groupId) =>
        {
            var group = await _groupsService.GetGroupById(groupId);
            if(group == null)
            {
                _chatBox.OutputTo(player, "Group doesn't exists");
                return;
            }
            _chatBox.OutputTo(player, $"Name: {group.Name}");
            _chatBox.OutputTo(player, $"Members: {string.Join(", ", group.Members.Select(x => x.UserId))}");
            _chatBox.OutputTo(player, $"Roles: {string.Join(", ", group.Roles.Select(x => new { x.Id, x.Name }))}");
        });

        _commandService.Add("setrole", async ([CallingPlayer] RealmPlayer player, int groupId, int roleId) =>
        {
            await _groupsService.SetMemberRole(groupId, player.UserId, roleId);
            _chatBox.OutputTo(player, $"Role changed");
        });

        _commandService.Add("creategrouprole", async ([CallingPlayer] RealmPlayer player, int groupId, string name) =>
        {
            var role = await _groupsService.CreateRole(groupId, name, []);
            _chatBox.OutputTo(player, $"Role created: {role.Id}");
        });

        _commandService.Add("setrolepermissions", async ([CallingPlayer] RealmPlayer player, int roleId, [ReadRestAsString] string arguments) =>
        {
            var permissions = arguments.Split(' ').Select(int.Parse).ToArray();
            var role = await _groupsService.SetRolePermissions(roleId, permissions);
            _chatBox.OutputTo(player, "Role permissions changed");
        });
    }

    private void AddHudCommands()
    {
        _commandService.Add("createclientblip", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Hud.CreateBlip(0, Vector3.Zero, Color.Red, 100, 1, 0, 0);
            player.Hud.CreateBlip(0, new Vector3(100, 100, 0), Color.Red, 100, 2, 0, 0);
        });
        
        _commandService.Add("createtempclientblip", async ([CallingPlayer] RealmPlayer player) =>
        {
            var blip = player.Hud.CreateBlip(0, Vector3.Zero, Color.Red, 100, 16, 0, 0);
            await Task.Delay(2500);
            player.Hud.RemoveBlip(blip);
        });
        
        _commandService.Add("removeclientblip", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Hud.RemoveAllBlips();
        });
        
        _commandService.Add("createhud", async ([CallingPlayer] RealmPlayer player) =>
        {
            var hud = player.Hud.AddLayer<SampleHudLayer>();
            if (hud == null)
                return;

            await Task.Delay(1000);
            hud.Offset = new Vector2(0, 100);
        });

        _commandService.Add("updatehud", ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.Hud.TryGetLayer(out SampleHudLayer layer))
            {
                layer.Update();
            }
        });
        
        _commandService.Add("hudresizerectangle", ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.Hud.TryGetLayer(out SampleHudLayer layer))
            {
                layer.Resize();
            }
        });
        
        _commandService.Add("hudrepositionrectangle", ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.Hud.TryGetLayer(out SampleHudLayer layer))
            {
                layer.SetPosition();
            }
        });
        
        _commandService.Add("hudrevisiblerectangle", ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.Hud.TryGetLayer(out SampleHudLayer layer))
            {
                layer.ToggleVisible();
            }
        });
        
        _commandService.Add("hudsetcontent", ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.Hud.TryGetLayer(out SampleHudLayer layer))
            {
                layer.ToggleContent();
            }
        });

        _commandService.Add("destroyhud", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Hud.TryRemoveLayer<SampleHudLayer>();
        });

        _commandService.Add("createHud3d", async ([CallingPlayer] RealmPlayer player) =>
        {
            var hud3d = _worldHudService.Create<SampleHud3d>(player.Position, 123);
            await Task.Delay(1000);
            hud3d.Update();
            hud3d.Dispose();
            _chatBox.OutputTo(player, "Hud3d created");
        });
    }

    void AddSettingsCommands()
    {
        _commandService.Add("setsetting", ([CallingPlayer] RealmPlayer player, string argument) =>
        {
            player.Settings.Set(1, argument);
            _chatBox.OutputTo(player, "Setting for vehicle changed");
        });

        _commandService.Add("removesetting", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Settings.TryRemove(1);
            _chatBox.OutputTo(player, "Setting removed");
        });

        _commandService.Add("getsetting", ([CallingPlayer] RealmPlayer player) =>
        {
            player.Settings.TryGet(1, out var value);
            _chatBox.OutputTo(player, $"Value of setting id 1: {value}");
        });

        _commandService.Add("vehiclesetsetting", ([CallingPlayer] RealmPlayer player, string argument) =>
        {
            if(player.FocusedElement is not RealmVehicle vehicle)
            {
                _chatBox.OutputTo(player, "You are not focused on vehicle.");
                return;
            }
            vehicle.Settings.Set(1, argument);
            _chatBox.OutputTo(player, "Setting for vehicle changed");
        });

        _commandService.Add("vehicleremovesetting", ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.FocusedElement is not RealmVehicle vehicle)
            {
                _chatBox.OutputTo(player, "You are not focused on vehicle.");
                return;
            }
            vehicle.Settings.TryRemove(1);
            _chatBox.OutputTo(player, "Setting removed");
        });

        _commandService.Add("vehiclegetsetting", ([CallingPlayer] RealmPlayer player) =>
        {
            if (player.FocusedElement is not RealmVehicle vehicle)
            {
                _chatBox.OutputTo(player, "You are not focused on vehicle.");
                return;
            }
            vehicle.Settings.TryGet(1, out var value);
            _chatBox.OutputTo(player, $"Value of setting id 1: {value}");
        });
    }


    void AddVehicleAccessCommands()
    {
        _commandService.Add("vehicleaccessinfo", ([CallingPlayer] RealmPlayer player) =>
        {
            var vehicle = GetVehicle(player);
            if (vehicle == null) return;

            _chatBox.OutputTo(player, "Access info:");

            foreach (var vehicleAccess in vehicle.Access)
            {
                if (vehicleAccess is VehicleUserAccessDto access)
                    _chatBox.OutputTo(player, $"User access: ({access.UserId}) = Ownership={access.IsOwner}");
                if (vehicleAccess is VehicleGroupAccessDto groupAccess)
                    _chatBox.OutputTo(player, $"Group access: ({groupAccess.GroupId})");
            }
        });

        _commandService.Add("vehicleaccessinfobyid", async ([CallingPlayer] RealmPlayer player, int id) =>
        {
            var vehicleRepository = player.GetRequiredService<VehiclesRepository>();
            var access = await vehicleRepository.GetAllAccesses(id);
            if (access == null)
            {
                _chatBox.OutputTo(player, "Vehicle not found");
                return;
            }

            _chatBox.OutputTo(player, "Access info:");

            foreach (var vehicleAccess in access)
            {
                if (vehicleAccess is VehicleUserAccessData userAccess)
                    _chatBox.OutputTo(player, $"User access: ({userAccess.UserId}) = Ownership={userAccess.AccessType == 0}");
                if (vehicleAccess is VehicleGroupAccessData groupAccess)
                    _chatBox.OutputTo(player, $"Group access: ({groupAccess.GroupId})");
            }
        });

        _commandService.Add("removeowner", ([CallingPlayer] RealmPlayer player) =>
        {
            var vehicle = GetVehicle(player);
            if (vehicle == null) return;

            player.Vehicle!.Access.TryRemoveAccess(player);
        });

        _commandService.Add("showaccesscontroller", ([CallingPlayer] RealmPlayer player) =>
        {
            var vehicle = GetVehicle(player);
            if (vehicle == null) return;

            _chatBox.OutputTo(player, vehicle?.AccessController?.ToString() ?? "brak");
        });

        _commandService.Add("addmeasowner", ([CallingPlayer] RealmPlayer player) =>
        {
            var vehicle = GetVehicle(player);
            if (vehicle == null) return;

            vehicle.Access.TryAddAsOwner(player);
        });

        _commandService.Add("accessaddgroup", ([CallingPlayer] RealmPlayer player, int groupId) =>
        {
            var vehicle = GetVehicle(player);
            if (vehicle == null) return;

            var added = vehicle.Access.TryAddGroupAccess(groupId, 1);
            if(added != null)
            {
                _chatBox.OutputTo(player, "Group added.");
            }
            else
            {
                _chatBox.OutputTo(player, "Failed to add group.");
            }
        });
    }

    private void AddOverlayCommands()
    {
        _commandService.Add("overlayCreateLine3d", ([CallingPlayer] RealmPlayer player) =>
        {
            var id1 = _overlayService.CreateLine3d([player], player.Position, player.Position + new Vector3(10, 0, 0), Color.Red, 2, Line3dEffect.Gravity(1));
            var id2 = _overlayService.CreateLine3d([player], player.Position, player.Position + new Vector3(10, 3, -3), Color.Red, 2, Line3dEffect.Gravity(2));
            var id3 = _overlayService.CreateLine3d([player], player.Position, player.Position + new Vector3(10, 6, -6), Color.Red, 2, Line3dEffect.Gravity(3));
            _chatBox.OutputTo(player, $"Line3d created of id: {id1}, {id2}, {id3}");
        });
        
        _commandService.Add("overlayCreateLine3dOffset", ([CallingPlayer] RealmPlayer player) =>
        {
            var id = _overlayService.CreateLine3d([player], new PositionContext(player, new(0,1,0)), player.Position + new Vector3(10, 0, 0), Color.Red, 2, Line3dEffect.Gravity(1));
            _chatBox.OutputTo(player, $"Line3dOffset created of id: {id}");
        });
        
        _commandService.Add("overlayCreateLine3dOffsetAttachedElement", ([CallingPlayer] RealmPlayer player) =>
        {
            var worldObject = _elementFactory.CreateObject(new Location(), ObjectModel.Gunbox);
            player.Attach(worldObject, BoneId.LeftHand, new Vector3(0.2f, 1.0f, -0), new Vector3(0, -20, 0));
            var id = _overlayService.CreateLine3d([player], new PositionContext(worldObject, new(0.3f,0.3f,0)), player.Position + new Vector3(10, 0, 0), Color.Red, 2, Line3dEffect.Gravity(1));

            _chatBox.OutputTo(player, $"Line3dOffsetElement created of id: {id}");
        });

        _commandService.Add("overlayCreateAndDestroyLine3d", async ([CallingPlayer] RealmPlayer player) =>
        {
            var id = _overlayService.CreateLine3d([player], player.Position, player.Position + new Vector3(10, 0, 0), Color.Red, 2, Line3dEffect.Gravity());
            _chatBox.OutputTo(player, $"Line3d created of id: {id}");
            await Task.Delay(2000);
            _overlayService.RemoveLine3d([player], [id]);
        });

        _commandService.Add("overlayAddEffect", ([CallingPlayer] RealmPlayer player) =>
        {
            _overlayService.AddEffect([player], ParticleEffect.BoatSplash, player.Position + new Vector3(2, 0, 0), Vector3.UnitZ, Color.Red, false, 8, 1, 0.3f, false, 1);
            _chatBox.OutputTo(player, "Effect created");
        });
    }

    private RealmVehicle? GetVehicle(RealmPlayer player)
    {
        if (player.Vehicle != null)
            return player.Vehicle;

        if (player.FocusedElement is RealmVehicle vehicle)
            return vehicle;

        _chatBox.OutputTo(player, "You are not focused on vehicle or not in vehicle.");
        return null;
    }

    public class SampleHud3d : WorldHud<SampleHudState>
    {
        private readonly AssetsCollection _assetsCollection;
        private readonly int _parameter;

        public SampleHud3d(AssetsCollection assetsCollection, int parameter) : base(new())
        {
            _assetsCollection = assetsCollection;
            _parameter = parameter;
        }

        protected override void Build(IHudBuilder builder, IHudBuilderContext context)
        {
            builder.Add(new RectangleHudElement(new Vector2(0,0), new Size(50, 20), Color.Red));
            builder.Add(new TextHudElement(CreateStatePropertyTextHudElement(x => $"string = {x.String} ({_parameter})"), new Vector2(0, 0), new Size(400, 20), font: BuildInFonts.Sans, alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center));
        }

        public void Update()
        {
            UpdateState(x =>
            {
                x.String = Guid.NewGuid().ToString();
            });
        }
    }

}

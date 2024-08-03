using RealmCore.BlazorGui.Modules.World;
using SlipeServer.Server.Elements;

namespace RealmCore.BlazorGui.Logic;

internal sealed partial class PlayerGameplayHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ChatBox _chatBox;
    private readonly VehicleUpgradesCollection _vehicleUpgradeCollection;
    private readonly VehicleEnginesCollection _vehicleEnginesCollection;
    private readonly UsersService _usersService;
    private readonly VehiclesService _vehiclesService;
    private readonly ILogger<PlayerGameplayHostedService> _logger;

    public PlayerGameplayHostedService(ILogger<PlayerGameplayHostedService> logger, IServiceProvider serviceProvider, ChatBox chatBox, VehicleUpgradesCollection vehicleUpgradeCollection, VehicleEnginesCollection vehicleEnginesCollection, UsersService usersService, VehiclesService vehiclesService)
    {
        _serviceProvider = serviceProvider;
        _chatBox = chatBox;
        _vehicleUpgradeCollection = vehicleUpgradeCollection;
        _vehicleEnginesCollection = vehicleEnginesCollection;
        _usersService = usersService;
        _vehiclesService = vehiclesService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _usersService.LoggedIn += HandleLoggedIn;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _usersService.LoggedIn += HandleLoggedIn;
        return Task.CompletedTask;
    }

    private Task HandleLoggedIn(RealmPlayer player)
    {
        player.FocusedElementChanged += HandleFocusedElementChanged;
        player.SetBindAsync("x", HandleInteract);
        return Task.CompletedTask;
    }

    private async Task HandleInteract(RealmPlayer player, KeyState keyState, CancellationToken cancellationToken)
    {
        var worldObject = player.AttachedBoneElements.FirstOrDefault()?.WorldObject;
        if (worldObject != null && keyState == KeyState.Down)
        {
            await player.DoAnimationAsync(Animation.CarryPutDown, cancellationToken: cancellationToken);

            var liftableInteraction = worldObject.Interaction as LiftableInteraction;
            if (liftableInteraction != null && liftableInteraction.TryDrop() && player.Detach(worldObject))
            {
                worldObject.Position = player.Position + player.Forward * 1.0f - new Vector3(0, 0, 0.55f);
                worldObject.Rotation = new Vector3
                {
                    X = 0,
                    Y = 0,
                    Z = player.Rotation.Z
                };
            }
        }
        else if (player.CurrentInteractElement != null)
        {
            var element = player.CurrentInteractElement;
            if (element == null)
                return;

            if (element is RealmWorldObject interactWorldObject && element is IInteractionHolder interactionHolder && interactionHolder.Interaction != null && player.DistanceTo(element) < interactionHolder.Interaction.MaxDistance)
            {
                switch (interactionHolder.Interaction)
                {
                    case SampleNodeInteraction sampleNodeInteraction when keyState == KeyState.Down:
                        {
                            await sampleNodeInteraction.SampleNode.HandleSampleInteraction();
                            _chatBox.OutputTo(player, "Action schedules");
                        }
                        break;
                    case LiftableInteraction liftableInteraction when keyState == KeyState.Down:
                        if (player.IsLookingAt(element) && liftableInteraction.TryLift(player))
                        {
                            await player.DoAnimationAsync(Animation.CarryLiftUp, cancellationToken: cancellationToken);
#pragma warning disable CA1849 // Call async methods when in an async method
                            player.DoAnimation(Animation.StartCarry);
#pragma warning restore CA1849 // Call async methods when in an async method
                            player.Attach(interactWorldObject, BoneId.LeftHand, new Vector3(0.2f, 0.2f, -0), new Vector3(0, -20, 0));
                            interactWorldObject.TrySetOwner(player);
                        }
                        break;
                    case DurationBasedHoldInteraction duractionBasedHoldInteraction:
                        _logger.LogInformation("Interaction begin {keyState}", keyState);
                        if (keyState == KeyState.Down)
                        {
                            using var token = new CancellationTokenSource();

                            void handleDestroyed(Element element)
                            {
                                token.Cancel();
                            }

                            void handleCurrentInteractElementChanged(RealmPlayer player, Element? fromElement, Element? toElement)
                            {
                                token.Cancel();
                            }

                            var currentInteractElement = player.CurrentInteractElement;
                            player.CurrentInteractElement.Destroyed += handleDestroyed;
                            player.CurrentInteractElementChanged += handleCurrentInteractElementChanged;

                            try
                            {
                                if (await duractionBasedHoldInteraction.BeginInteraction(player, token.Token))
                                {
                                    _chatBox.OutputTo(player, "okk");
                                    _logger.LogInformation("Interaction completed");
                                }
                                else
                                {
                                    _logger.LogInformation("Interaction failed");
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                _logger.LogInformation("Interaction failed ( not focused )");
                            }
                            finally
                            {
                                currentInteractElement.Destroyed -= handleDestroyed;
                                player.CurrentInteractElementChanged -= handleCurrentInteractElementChanged;
                                await token.CancelAsync();
                            }
                        }
                        else
                        {
                            if (duractionBasedHoldInteraction.EndInteraction(player))
                            {
                                _logger.LogInformation("Interaction ended");
                            }
                            else
                            {
                                _logger.LogInformation("Interaction end failed");
                            }
                        }
                        ;
                        break;

                }
            }
        }
    }

    private void HandleFocusedElementChanged(RealmPlayer player, Element? previous, Element? element)
    {
        if (element is RealmVehicleForSale realmVehicleForSale)
        {
            if (player.Gui.Current == null)
            {
                var vehicle = (RealmVehicle)element;
                var vehicleName = ((RealmVehicle)element).Name;
                var buyVehicleGui = new BuyVehicleGui(player, vehicleName, realmVehicleForSale.Price)
                {
                    //Bought = async () =>
                    //{
                    //    player.Gui.TryClose<BuyVehicleGui>();
                    //    if (realmVehicleForSale.TrySell())
                    //    {
                    //        vehicle = await _vehiclesService.ConvertToPersistantVehicle(vehicle);
                    //        if (vehicle == null)
                    //            throw new NullReferenceException();
                    //        vehicle.Access.AddAsOwner(player);
                    //        vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
                    //    }
                    //}
                };
                player.Gui.Current = buyVehicleGui;
            }
        }
        if (element is IInteractionHolder interaction)
        {
            if (interaction.Interaction != null)
            {
                player.CurrentInteractElement = element;
            }
        }
        else
        {
            player.CurrentInteractElement = null;
            player.Gui.TryClose<BuyVehicleGui>();
        }
    }
}

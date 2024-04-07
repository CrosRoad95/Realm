using RealmCore.Sample.Concepts.Gui;
using RealmCore.Sample.Elements;
using SlipeServer.Server.Elements.Enums;

namespace RealmCore.Sample.Logic;

internal sealed partial class PlayerGameplayLogic
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ChatBox _chatBox;
    private readonly VehicleUpgradesCollection _vehicleUpgradeCollection;
    private readonly VehicleEnginesCollection _vehicleEnginesCollection;
    private readonly IVehiclesService _vehiclesService;
    private readonly ILogger<PlayerGameplayLogic> _logger;

    public PlayerGameplayLogic(ILogger<PlayerGameplayLogic> logger, IServiceProvider serviceProvider, ChatBox chatBox, VehicleUpgradesCollection vehicleUpgradeCollection, VehicleEnginesCollection vehicleEnginesCollection, IUsersService usersService, IVehiclesService vehiclesService)
    {
        _serviceProvider = serviceProvider;
        _chatBox = chatBox;
        _vehicleUpgradeCollection = vehicleUpgradeCollection;
        _vehicleEnginesCollection = vehicleEnginesCollection;
        _vehiclesService = vehiclesService;
        _logger = logger;
        usersService.SignedIn += HandleSignedIn;
    }

    private void HandleSignedIn(RealmPlayer player)
    {
        player.FocusedElementChanged += HandleFocusedElementChanged;
        player.SetBindAsync("x", HandleInteract);
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
                    case LiftableInteraction liftableInteraction when keyState == KeyState.Down:
                        if (player.IsLookingAt(element) && liftableInteraction.TryLift(player))
                        {
                            await player.DoAnimationAsync(Animation.CarryLiftUp, cancellationToken: cancellationToken);
#pragma warning disable CA1849 // Call async methods when in an async method
                            player.DoAnimation(Animation.StartCarry);
#pragma warning restore CA1849 // Call async methods when in an async method
                            player.Attach(interactWorldObject, SlipeServer.Packets.Enums.BoneId.LeftHand, new Vector3(0.2f, 0.2f, -0), new Vector3(0, -20, 0));
                            interactWorldObject.TrySetOwner(player);
                        }
                        break;
                    case DurationBasedHoldInteraction duractionBasedHoldInteraction:
                        _logger.LogInformation("Interaction begin {keyState}", keyState);
                        if (keyState == KeyState.Down)
                        {
                            var token = new CancellationTokenSource();
                            player.CurrentInteractElement.Destroyed += e =>
                            {
                                token.Cancel();
                            };
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
        if(element is RealmVehicleForSale realmVehicleForSale)
        {
            if (player.Gui.Current == null)
            {
                var vehicle = ((RealmVehicle)element);
                var vehicleName = ((RealmVehicle)element).Name;
                var buyVehicleGui = new BuyVehicleGui(player, vehicleName, realmVehicleForSale.Price)
                {
                    Bought = async () =>
                    {
                        player.Gui.Close<BuyVehicleGui>();
                        if (realmVehicleForSale.TrySell())
                        {
                            vehicle = await _vehiclesService.ConvertToPersistantVehicle(vehicle);
                            vehicle.Access.AddAsOwner(player);
                            vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
                        }
                    }
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
            player.Gui.Close<BuyVehicleGui>();
        }
    }
}

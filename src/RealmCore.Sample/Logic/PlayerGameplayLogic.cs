using RealmCore.Sample.Elements;
using RealmCore.Server.Components.Abstractions;
using RealmCore.Server.Enums;
using SlipeServer.Server.Elements.Enums;

namespace RealmCore.Sample.Logic;

internal sealed partial class PlayerGameplayLogic
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ChatBox _chatBox;
    private readonly VehicleUpgradeRegistry _vehicleUpgradeRegistry;
    private readonly VehicleEnginesRegistry _vehicleEnginesRegistry;
    private readonly ILogger<PlayerGameplayLogic> _logger;

    public PlayerGameplayLogic(ILogger<PlayerGameplayLogic> logger, IServiceProvider serviceProvider, ChatBox chatBox, VehicleUpgradeRegistry vehicleUpgradeRegistry, VehicleEnginesRegistry vehicleEnginesRegistry, IUsersService usersService)
    {
        _serviceProvider = serviceProvider;
        _chatBox = chatBox;
        _vehicleUpgradeRegistry = vehicleUpgradeRegistry;
        _vehicleEnginesRegistry = vehicleEnginesRegistry;
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
        var components = player;
        if (components.TryGetComponent(out AttachedElementComponent attachedElementComponent) && keyState == KeyState.Down)
        {
            await player.DoAnimationAsync(Animation.CarryPutDown);

            var detached = attachedElementComponent.TryDetach(out Element? detachedElement);
            if (detached && detachedElement != null)
            {
                if (detachedElement.GetRequiredComponent<LiftableWorldObjectComponent>().TryDrop())
                {
                    detachedElement.Position = player.Position + player.Forward * 1.0f - new Vector3(0, 0, 0.55f);
                    detachedElement.Rotation = new Vector3
                    {
                        X = 0,
                        Y = 0,
                        Z = player.Rotation.Z
                    };
                }
            }
        }
        else if (player.CurrentInteractElement != null)
        {
            var element = player.CurrentInteractElement;
            if (element == null)
                return;

            var currentInteractElement = element as IComponents;
            if (currentInteractElement == null)
                return;

            if (currentInteractElement.TryGetComponent(out InteractionComponent interactionComponent) && player.DistanceTo(element) < interactionComponent.MaxInteractionDistance)
            {
                switch (interactionComponent)
                {
                    case LiftableWorldObjectComponent liftableWorldObjectComponent when keyState == KeyState.Down:
                        if (player.IsLookingAt(element) && liftableWorldObjectComponent.TryLift(player))
                        {
                            await player.DoAnimationAsync(Animation.CarryLiftUp);
#pragma warning disable CA1849 // Call async methods when in an async method
                            player.DoAnimation(Animation.StartCarry);
#pragma warning restore CA1849 // Call async methods when in an async method
                            player.AddComponent(new AttachedElementComponent(element, SlipeServer.Packets.Enums.BoneId.LeftHand, new Vector3(0.2f, 0.2f, -0), new Vector3(0, -20, 0)));
                        }
                        break;
                    case DurationBasedHoldInteractionComponent durationBasedHoldInteractionComponent:
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
                                if (await durationBasedHoldInteractionComponent.BeginInteraction(player, token.Token))
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
                            if (durationBasedHoldInteractionComponent.EndInteraction(player))
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

    private void HandleFocusedElementChanged(RealmPlayer player, Element? element)
    {
        if(element is RealmVehicleForSale realmVehicleForSale)
        {
            if (player.Gui.Current == null)
            {
                var vehicle = ((RealmVehicle)element);
                var vehicleName = ((RealmVehicle)element).Name;
                var buyVehicleGui = new BuyVehicleGui(player, vehicleName, realmVehicleForSale.Price);
                buyVehicleGui.Bought = async () =>
                {
                    player.Gui.Close<BuyVehicleGui>();
                    if (realmVehicleForSale.TrySell())
                    {
                        await player.GetRequiredService<IVehiclesService>().ConvertToPersistantVehicle(vehicle);
                        vehicle.Access.AddAsOwner(player);
                        vehicle.Fuel.AddFuelContainer(1, 20, 20, 0.01f, 2, true);
                    }
                };
                player.Gui.Current = buyVehicleGui;
            }
        }
        if (element is IComponents components)
        {
            if (components.Components.TryGetComponent(out InteractionComponent interactionComponent))
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

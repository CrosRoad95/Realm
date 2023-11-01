using RealmCore.Sample.Components.Vehicles;
using RealmCore.Server.Components.Abstractions;
using RealmCore.Server.Enums;
using SlipeServer.Server.Elements.Enums;

namespace RealmCore.Sample.Logic;

internal sealed class PlayerGameplayLogic : ComponentLogic<UserComponent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ChatBox _chatBox;
    private readonly VehicleUpgradeRegistry _vehicleUpgradeRegistry;
    private readonly VehicleEnginesRegistry _vehicleEnginesRegistry;
    private readonly ILogger<PlayerGameplayLogic> _logger;

    public PlayerGameplayLogic(ILogger<PlayerGameplayLogic> logger, IServiceProvider serviceProvider, ChatBox chatBox, VehicleUpgradeRegistry vehicleUpgradeRegistry, VehicleEnginesRegistry vehicleEnginesRegistry, IElementFactory elementFactory) : base(elementFactory)
    {
        _serviceProvider = serviceProvider;
        _chatBox = chatBox;
        _vehicleUpgradeRegistry = vehicleUpgradeRegistry;
        _vehicleEnginesRegistry = vehicleEnginesRegistry;
        _logger = logger;
    }

    protected override void ComponentAdded(UserComponent userComponent)
    {
        var player = (RealmPlayer)userComponent.Element;
        player.FocusedElementChanged += HandleFocusedElementChanged;
        player.SetBindAsync("x", HandleInteract);
    }

    private async Task HandleInteract(RealmPlayer player, KeyState keyState)
    {
        var components = player.Components;
        if (components.TryGetComponent(out AttachedElementComponent attachedElementComponent))
        {
            await player.DoAnimationAsync(Animation.CarryPutDown);

            if (attachedElementComponent.TryDetach(out Element? detachedElement) &&
                detachedElement is IComponents elementComponents &&
                elementComponents.Components.GetRequiredComponent<LiftableWorldObjectComponent>().TryDrop())
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
        else if (player.Components.TryGetComponent(out CurrentInteractElementComponent currentInteractElementComponent))
        {
            var element = currentInteractElementComponent.CurrentInteractElement;
            var currentInteractElement = currentInteractElementComponent.CurrentInteractElement as IComponents;
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
                            player.DoAnimation(Animation.StartCarry);
                            player.Components.AddComponent(new AttachedElementComponent(element, SlipeServer.Packets.Enums.BoneId.LeftHand, new Vector3(0.2f, 0.2f, -0), new Vector3(0, -20, 0)));
                        }
                        break;
                    case DurationBasedHoldInteractionComponent durationBasedHoldInteractionComponent:
                        _logger.LogInformation("Interaction begin {keyState}", keyState);
                        if (keyState == KeyState.Down)
                        {
                            var token = new CancellationTokenSource();
                            currentInteractElementComponent.Disposed += e =>
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
        if (element is IComponents components)
        {
            if (components.Components.TryGetComponent(out VehicleForSaleComponent vehicleForSaleComponent))
            {
                if (!player.Components.HasComponent<GuiComponent>())
                {
                    var vehicle = ((RealmVehicle)element);
                    var vehicleName = ((RealmVehicle)element).Name;
                    player.Components.AddComponent(new BuyVehicleGuiComponent(vehicleName, vehicleForSaleComponent.Price)).Bought = async () =>
                    {
                        player.Components.TryDestroyComponent<BuyVehicleGuiComponent>();
                        if (vehicle.Components.TryDestroyComponent<VehicleForSaleComponent>())
                        {
                            await _serviceProvider.GetRequiredService<IVehiclesService>().ConvertToPrivateVehicle(vehicle);
                            vehicle.Components.GetRequiredComponent<PrivateVehicleComponent>().Access.AddAsOwner(player);
                            vehicle.Components.AddComponent<VehicleUpgradesComponent>();
                            vehicle.Components.AddComponent(new MileageCounterComponent());
                            vehicle.Components.AddComponent(new FuelComponent(1, 20, 20, 0.01, 2)).Active = true;
                        }
                    };
                }
            }
            else if (components.Components.TryGetComponent(out InteractionComponent interactionComponent))
            {
                player.Components.TryDestroyComponent<CurrentInteractElementComponent>();
                player.Components.AddComponent(new CurrentInteractElementComponent(element));
            }
        }
        else
        {
            player.Components.TryDestroyComponent<CurrentInteractElementComponent>();
            player.Components.TryDestroyComponent<BuyVehicleGuiComponent>();
        }
    }
}

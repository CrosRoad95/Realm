using RealmCore.Console.Components.Vehicles;
using RealmCore.ECS;
using RealmCore.ECS.Components;
using RealmCore.Server.Components.Elements.Abstractions;
using RealmCore.Server.Components.Players.Abstractions;
using RealmCore.Server.Enums;
using SlipeServer.Server.Elements.Enums;

namespace RealmCore.Console.Logic;

internal sealed class PlayerGameplayLogic
{
    private readonly IEntityEngine _ecs;
    private readonly IServiceProvider _serviceProvider;
    private readonly ChatBox _chatBox;
    private readonly VehicleUpgradeRegistry _vehicleUpgradeRegistry;
    private readonly VehicleEnginesRegistry _vehicleEnginesRegistry;
    private readonly ILogger<PlayerGameplayLogic> _logger;

    public PlayerGameplayLogic(IEntityEngine ecs, ILogger<PlayerGameplayLogic> logger, IServiceProvider serviceProvider, ChatBox chatBox, VehicleUpgradeRegistry vehicleUpgradeRegistry, VehicleEnginesRegistry vehicleEnginesRegistry)
    {
        _ecs = ecs;
        _serviceProvider = serviceProvider;
        _chatBox = chatBox;
        _vehicleUpgradeRegistry = vehicleUpgradeRegistry;
        _vehicleEnginesRegistry = vehicleEnginesRegistry;
        _logger = logger;
        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            return;


        entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is UserComponent)
        {
            var playerElementComponent = component.Entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.FocusedEntityChanged += HandleFocusedEntityChanged;
            playerElementComponent.SetBindAsync("x", HandleInteract);
        }
    }

    private async Task HandleInteract(Entity playerEntity, KeyState keyState)
    {
        if (playerEntity.TryGetComponent(out AttachedEntityComponent attachedEntityComponent))
        {
            await playerEntity.GetRequiredComponent<PlayerElementComponent>().DoAnimationAsync(Animation.CarryPutDown);

            if (attachedEntityComponent.TryDetach(out Entity? detachedEntity) &&
                detachedEntity != null &&
                detachedEntity.GetRequiredComponent<LiftableWorldObjectComponent>().TryDrop())
            {
                detachedEntity.Transform.Position = playerEntity.Transform.Position + playerEntity.Transform.Forward * 1.0f - new Vector3(0, 0, 0.55f);
                detachedEntity.Transform.Rotation = new Vector3
                {
                    X = 0,
                    Y = 0,
                    Z = playerEntity.Transform.Rotation.Z
                };
            }
        }
        else if (playerEntity.TryGetComponent(out CurrentInteractEntityComponent currentInteractEntityComponent))
        {
            var currentInteractEntity = currentInteractEntityComponent.CurrentInteractEntity;
            if (currentInteractEntity == null)
                return;

            if (currentInteractEntity.TryGetComponent(out InteractionComponent interactionComponent) && playerEntity.DistanceTo(currentInteractEntity) < interactionComponent.MaxInteractionDistance)
            {
                var playerElementComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
                switch (interactionComponent)
                {
                    case LiftableWorldObjectComponent liftableWorldObjectComponent when keyState == KeyState.Down:
                        if (playerEntity.IsLookingAt(currentInteractEntity) && liftableWorldObjectComponent.TryLift(playerEntity))
                        {
                            await playerElementComponent.DoAnimationAsync(Animation.CarryLiftUp);
                            playerElementComponent.DoAnimation(Animation.StartCarry);
                            playerEntity.AddComponent(new AttachedEntityComponent(currentInteractEntity, SlipeServer.Packets.Enums.BoneId.LeftHand, new Vector3(0.2f, 0.2f, -0), new Vector3(0, -20, 0)));
                        }
                        break;
                    case DurationBasedHoldInteractionComponent durationBasedHoldInteractionComponent:
                        _logger.LogInformation("Interaction begin {keyState}", keyState);
                        if (keyState == KeyState.Down)
                        {
                            var token = new CancellationTokenSource();
                            currentInteractEntityComponent.Disposed += e =>
                            {
                                token.Cancel();
                            };
                            try
                            {
                                if (await durationBasedHoldInteractionComponent.BeginInteraction(playerEntity, token.Token))
                                {
                                    _chatBox.OutputTo(playerEntity, "okk");
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
                            if (durationBasedHoldInteractionComponent.EndInteraction(playerEntity))
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

    private void HandleFocusedEntityChanged(Entity playerEntity, Entity? entity)
    {
        if (entity != null)
        {
            if (entity.TryGetComponent(out VehicleForSaleComponent vehicleForSaleComponent))
            {
                if (!playerEntity.HasComponent<GuiComponent>())
                {
                    var vehicleName = entity.GetRequiredComponent<VehicleElementComponent>().Name;
                    playerEntity.AddComponent(new BuyVehicleGuiComponent(vehicleName, vehicleForSaleComponent.Price)).Bought = async () =>
                    {
                        playerEntity.TryDestroyComponent<BuyVehicleGuiComponent>();
                        if (entity.TryDestroyComponent<VehicleForSaleComponent>())
                        {
                            await _serviceProvider.GetRequiredService<IVehiclesService>().ConvertToPrivateVehicle(entity);
                            entity.GetRequiredComponent<PrivateVehicleComponent>().Access.AddAsOwner(playerEntity);
                            entity.AddComponent(new VehicleUpgradesComponent(_vehicleUpgradeRegistry, _vehicleEnginesRegistry));
                            entity.AddComponent(new MileageCounterComponent());
                            entity.AddComponent(new FuelComponent(1, 20, 20, 0.01, 2)).Active = true;
                        }
                    };
                }
            }
            else if (entity.TryGetComponent(out InteractionComponent interactionComponent))
            {
                playerEntity.TryDestroyComponent<CurrentInteractEntityComponent>();
                playerEntity.AddComponent(new CurrentInteractEntityComponent(entity));
            }
        }
        else
        {
            playerEntity.TryDestroyComponent<CurrentInteractEntityComponent>();
            playerEntity.TryDestroyComponent<BuyVehicleGuiComponent>();
        }
    }
}

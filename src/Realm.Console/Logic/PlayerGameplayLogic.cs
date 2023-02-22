using Realm.Server.Extensions;
using SlipeServer.Server.Elements.Enums;

namespace Realm.Console.Logic;

internal sealed class PlayerGameplayLogic
{
    private readonly ECS _ecs;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlayerGameplayLogic> _logger;

    public PlayerGameplayLogic(ECS ecs, ILogger<PlayerGameplayLogic> logger, IServiceProvider serviceProvider)
    {
        _ecs = ecs;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != Entity.EntityTag.Player)
            return;


        entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if(component is AccountComponent)
        {
            var playerElementComponent = component.Entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.FocusedEntityChanged += HandleFocusedEntityChanged;
            playerElementComponent.SetBind("x", HandleInteract);
        }
    }

    private async Task HandleInteract(Entity playerEntity, KeyState keyState)
    {
        if(playerEntity.TryGetComponent(out AttachedEntityComponent attachedEntityComponent))
        {
            await playerEntity.GetRequiredComponent<PlayerElementComponent>().DoAnimationAsync(PlayerElementComponent.Animation.CarryPutDown);

            if (attachedEntityComponent.AttachedEntity.GetRequiredComponent<LiftableWorldObjectComponent>().TryDrop())
            {
                playerEntity.DestroyComponent(attachedEntityComponent);
                attachedEntityComponent.AttachedEntity.Transform.Position = playerEntity.Transform.Position + playerEntity.Transform.Forward * 1.0f - new Vector3(0, 0, 0.55f);
                attachedEntityComponent.AttachedEntity.Transform.Rotation = new Vector3
                {
                    X = 0,
                    Y = 0,
                    Z = playerEntity.Transform.Rotation.Z
                };
            }
        }
        else if(playerEntity.TryGetComponent(out CurrentInteractEntityComponent currentInteractEntityComponent))
        {
            var currentInteractEntity = currentInteractEntityComponent.CurrentInteractEntity;
            if (currentInteractEntity.TryGetComponent(out InteractionComponent interactionComponent) && playerEntity.DistanceTo(currentInteractEntity) < interactionComponent.MaxInteractionDistance)
            {
                var playerElementComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
                switch (interactionComponent)
                {
                    case LiftableWorldObjectComponent liftableWorldObjectComponent when keyState == KeyState.Down:
                        if (playerEntity.IsLookingAt(currentInteractEntity) && liftableWorldObjectComponent.TryLift(playerEntity))
                        {
                            await playerElementComponent.DoAnimationAsync(PlayerElementComponent.Animation.CarryLiftUp);
                            playerElementComponent.DoAnimation(PlayerElementComponent.Animation.StartCarry);
                            playerEntity.AddComponent(new AttachedEntityComponent(currentInteractEntity, SlipeServer.Packets.Enums.BoneId.LeftHand, new Vector3(0.2f, 0.2f, -0), new Vector3(0, -20, 0)));
                        }
                        break;
                    case DurationBasedHoldInteractionComponent durationBasedHoldInteractionComponent:
                        _logger.LogInformation("Interaction begin");
                        if (keyState == KeyState.Down)
                        {
                            var token = new CancellationTokenSource();
                            currentInteractEntityComponent.Disposed += e =>
                            {
                                token.Cancel();
                            };
                            try
                            {
                                if (await durationBasedHoldInteractionComponent.BeginInteraction(currentInteractEntity, TimeSpan.FromSeconds(5), token.Token))
                                {
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
                            if (durationBasedHoldInteractionComponent.EndInteraction(currentInteractEntity))
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
                if(!playerEntity.HasComponent<GuiComponent>())
                {
                    var vehicleName = entity.GetRequiredComponent<VehicleElementComponent>().Name;
                    playerEntity.AddComponent(new BuyVehicleGuiComponent(vehicleName, vehicleForSaleComponent.Price)).Bought = async () =>
                    {
                        playerEntity.TryDestroyComponent<BuyVehicleGuiComponent>();
                        if(entity.TryDestroyComponent<VehicleForSaleComponent>())
                        {
                            await _serviceProvider.GetRequiredService<IVehiclesService>().ConvertToPrivateVehicle(entity);
                            entity.GetRequiredComponent<PrivateVehicleComponent>().AddAsOwner(playerEntity);
                            entity.AddComponent(new VehicleUpgradesComponent());
                            entity.AddComponent(new MileageCounterComponent());
                            entity.AddComponent(new VehicleFuelComponent("default", 20, 20, 0.01, 2)).Active = true;
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

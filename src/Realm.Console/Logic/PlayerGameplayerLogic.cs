using Realm.Server.Extensions;

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

        var playerElementComponent =  entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.FocusedEntityChanged += HandleFocusedEntityChanged;
        playerElementComponent.SetBind("x", HandleInteract);
    }

    private async Task HandleInteract(Entity playerEntity)
    {
        if(playerEntity.TryGetComponent(out AttachedEntityComponent attachedEntityComponent))
        {
            await playerEntity.GetRequiredComponent<PlayerElementComponent>().DoAnimationAsync(PlayerElementComponent.Animation.CarryPutDown);

            if (attachedEntityComponent.AttachedEntity.GetRequiredComponent<LiftableWorldObjectComponent>().TryDrop())
            {
                playerEntity.DestroyComponent(attachedEntityComponent);
                attachedEntityComponent.AttachedEntity.Transform.Position += new Vector3(0.0f, 0.0f, -0.6f);
                attachedEntityComponent.AttachedEntity.Transform.Rotation = new Vector3
                {
                    X = 0,
                    Y = 0,
                    Z = 0
                };
            }
        }
        else if(playerEntity.TryGetComponent(out CurrentInteractEntityComponent currentInteractEntityComponent))
        {
            var currentInteractEntity = currentInteractEntityComponent.CurrentInteractEntity;
            if (playerEntity.DistanceTo(currentInteractEntity) < 1.3f && currentInteractEntity.TryGetComponent(out LiftableWorldObjectComponent liftableWorldObjectComponent))
            {
                if (playerEntity.IsLookingAt(currentInteractEntity) && liftableWorldObjectComponent.TryLift(playerEntity))
                {
                    await playerEntity.GetRequiredComponent<PlayerElementComponent>().DoAnimationAsync(PlayerElementComponent.Animation.CarryLiftUp);
                    playerEntity.GetRequiredComponent<PlayerElementComponent>().DoAnimation(PlayerElementComponent.Animation.StartCarry);
                    playerEntity.AddComponent(new AttachedEntityComponent(currentInteractEntity, new Vector3(0.6f, 0.6f, -0.4f)));
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
            else if (entity.TryGetComponent(out LiftableWorldObjectComponent liftableWorldObjectComponent))
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

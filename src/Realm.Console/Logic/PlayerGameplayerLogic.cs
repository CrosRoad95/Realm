using Microsoft.Extensions.DependencyInjection;
using Realm.Console.Components.Players;
using Realm.Domain.Components.Object;

namespace Realm.Console.Logic;

internal sealed class PlayerGameplayLogic
{
    private readonly ECS _ecs;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public PlayerGameplayLogic(ECS ecs, ILogger logger, IServiceProvider serviceProvider)
    {
        _ecs = ecs;
        _serviceProvider = serviceProvider;
        _logger = logger.ForContext<PlayerJoinedLogic>();
        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != Entity.PlayerTag)
            return;

        var playerElementComponent =  entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.FocusedEntityChanged += HandleFocusedEntityChanged;
        playerElementComponent.SetBind("x", HandleInteract);
    }

    private Task HandleInteract(Entity entity)
    {
        if(entity.TryGetComponent(out CurrentInteractEntityComponent currentInteractEntityComponent))
        {
            ;
        }
        return Task.CompletedTask;
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

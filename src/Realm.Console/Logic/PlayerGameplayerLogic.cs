using Microsoft.Extensions.DependencyInjection;

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

    private async void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != Entity.PlayerTag)
            return;

        entity.GetRequiredComponent<PlayerElementComponent>().FocusedEntityChanged += HandleFocusedEntityChanged;

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
        }
        else
        {
            if (playerEntity.TryDestroyComponent<BuyVehicleGuiComponent>())
            {

            }
        }
    }
}

namespace Realm.Console.Logic;

internal sealed class PlayerGameplayLogic
{
    private readonly ECS _ecs;
    private readonly ILogger _logger;

    public PlayerGameplayLogic(ECS ecs, ILogger logger)
    {
        _ecs = ecs;
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
                playerEntity.AddComponent(new BuyVehicleGuiComponent());
            }
        }
        else
        {
            playerEntity.TryDestroyComponent<BuyVehicleGuiComponent>();
        }
    }
}

namespace Realm.Console.Logic;

internal sealed class PlayerBindsLogic
{
    private readonly ECS _ecs;

    public PlayerBindsLogic(ECS ecs)
    {
        _ecs = ecs;

        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if(entity.Tag == Entity.PlayerTag)
            entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is AccountComponent)
        {
            var entity = component.Entity;
            var playerElementComponent = component.Entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.SetBind("F1", e =>
            {
                if(entity.HasComponent<DashboardGuiComponent>())
                {
                    entity.DestroyComponent<DashboardGuiComponent>();
                    return Task.CompletedTask;
                }

                if(entity.HasComponent<GuiComponent>())
                {
                    playerElementComponent.AddNotification("Nie możesz otworzyć tego panelu ponieważ masz inne gui aktywne.");
                    return Task.CompletedTask;
                }

                entity.AddComponent(new DashboardGuiComponent());
                playerElementComponent.ResetCooldown("F1");
                return Task.CompletedTask;
            });
        }
    }
}

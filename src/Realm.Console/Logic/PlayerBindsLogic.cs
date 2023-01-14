namespace Realm.Console.Logic;

internal sealed class PlayerBindsLogic
{
    public struct GuiOpenOptions
    {
        public List<Type>? allowToOpenWithGui;
    }

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

    private void OpenCloseGuiHelper<TGuiComponent>(Entity entity, string bind, GuiOpenOptions options = default) where TGuiComponent: GuiComponent, new()
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetBind(bind, async entity =>
        {
            if (entity.HasComponent<TGuiComponent>())
            {
                entity.DestroyComponent<TGuiComponent>();
                return;
            }

            if (entity.HasComponent<GuiComponent>())
                entity.DestroyComponent<GuiComponent>();

            var guiComponent = await entity.AddComponentAsync(new TGuiComponent());
            playerElementComponent.ResetCooldown(bind);
        });
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is AccountComponent)
        {
            var entity = component.Entity;
            OpenCloseGuiHelper<DashboardGuiComponent>(entity, "F1");
            OpenCloseGuiHelper<InventoryGuiComponent>(entity, "i");
        }
    }
}

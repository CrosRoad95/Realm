namespace Realm.Console.Logic;

internal sealed class PlayerBindsLogic
{
    private readonly IInternalRPGServer _rpgServer;

    public PlayerBindsLogic(IInternalRPGServer rpgServer)
    {
        _rpgServer = rpgServer;

        _rpgServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Entity entity)
    {
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
                return Task.CompletedTask;
            });
        }
    }
}

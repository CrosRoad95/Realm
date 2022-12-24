namespace Realm.Console.Logic;

internal sealed class PlayerJoinedLogic
{
    private readonly IInternalRPGServer _rpgServer;

    public PlayerJoinedLogic(IInternalRPGServer rpgServer)
    {
        _rpgServer = rpgServer;

        _rpgServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Entity entity)
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        entity.AddComponent(new AdminComponent()).DebugView = true;
        entity.AddComponent(new LoginGuiComponent());

        entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if(component is AccountComponent)
        {
            var playerElementComponent = component.Entity.GetRequiredComponent<PlayerElementComponent>();
            playerElementComponent.Spawn(new Vector3(0, 0, 4));
        }
    }
}

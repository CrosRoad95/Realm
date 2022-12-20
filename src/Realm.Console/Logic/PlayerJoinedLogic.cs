using Realm.Domain.Components.Players;
using Realm.Domain.Components.Players.Gui;
using Realm.Server.Interfaces;

namespace Realm.Console.Logic;

internal class PlayerJoinedLogic
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
        playerElementComponent.Spawn(new Vector3(0,0, 4));
        entity.AddComponent(new LoginGuiComponent());
    }
}

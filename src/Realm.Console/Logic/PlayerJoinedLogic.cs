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
        entity.GetRequiredComponent<PlayerElementComponent>().Spawn(new Vector3(0,0, 4));
    }
}

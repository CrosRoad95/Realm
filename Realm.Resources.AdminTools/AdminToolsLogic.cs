using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;

namespace Realm.Resources.AdminTools;

internal class AdminToolsLogic
{
    private readonly AdminToolsResource _resource;

    public AdminToolsLogic(MtaServer mtaServer)
    {
        mtaServer.PlayerJoined += HandlePlayerJoin;
        _resource = mtaServer.GetAdditionalResource<AdminToolsResource>();
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}
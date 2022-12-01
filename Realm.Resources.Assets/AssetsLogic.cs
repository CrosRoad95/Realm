using SlipeServer.Server.Elements;
using SlipeServer.Server;

namespace Realm.Resources.Assets;

internal class AssetsLogic
{
    private readonly AssetsResource _resource;

    public AssetsLogic(MtaServer server)
    {
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<AssetsResource>();
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

}

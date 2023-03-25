using SlipeServer.Server.Elements;

namespace Realm.Resources.AdminTools;

public interface IAdminToolsService
{
    internal event Action<Player>? AdminToolsEnabled;
    internal event Action<Player>? AdminToolsDisabled;

    void DisableAdminToolsForPlayer(Player player);
    void EnableAdminToolsForPlayer(Player player);
    bool HasAdminToolsEnabled(Player player);
}

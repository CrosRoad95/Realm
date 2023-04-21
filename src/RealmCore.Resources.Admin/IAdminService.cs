using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Admin;

public interface IAdminService
{
    internal event Action<Player>? AdminEnabled;
    internal event Action<Player>? AdminDisabled;

    void DisableAdminModeForPlayer(Player player);
    void EnableAdminModeForPlayer(Player player);
    bool HasAdminModeEnabled(Player player);
}

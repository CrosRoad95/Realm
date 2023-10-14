using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Browser;

public interface IBrowserService
{
    event Action<Player>? PlayerBrowserStarted;
    event Action<Player>? PlayerBrowserStopped;

    Action<IMessage>? MessageHandler { get; set; }
    Action<Player>? RelayPlayerBrowserReady { get; set; }

    internal void HandlePlayerBrowserReady(Player player);

    void ToggleDevTools(Player player, bool enabled);
    void SetVisible(Player player, bool visible);
    void SetPath(Player player, string path, bool force);
}

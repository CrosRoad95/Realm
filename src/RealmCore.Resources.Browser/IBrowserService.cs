using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Browser;

public interface IBrowserService
{
    event Action<Player>? BrowserStarted;
    event Action<Player>? BrowserStopped;
    event Action<Player>? BrowserReady;

    Action<IMessage>? MessageHandler { get; set; }

    internal void RelayBrowserReady(Player player);

    void ToggleDevTools(Player player, bool enabled);
    void SetVisible(Player player, bool visible);
    void SetPath(Player player, string path);
    void RelayBrowserStarted(Player player);
}

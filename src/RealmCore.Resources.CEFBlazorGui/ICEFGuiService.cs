using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.CEFBlazorGui;

public interface ICEFBlazorGuiService
{
    event Action<Player>? PlayerCEFBlazorGuiStarted;
    event Action<Player>? PlayerCEFBlazorGuiStopped;

    Action<IMessage>? MessageHandler { get; set; }
    Action<Player>? RelayPlayerBrowserReady { get; set; }
    Action<Player>? RelayPlayerBlazorReady { get; set; }

    internal void HandlePlayerBrowserReady(Player player);

    void ToggleDevTools(Player player, bool enabled);
    void SetVisible(Player player, bool visible);
    void SetPath(Player player, string path, bool force, bool isAsync);
    void SetRemotePath(Player player, string path);
}

using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.CEFBlazorGui;

public interface ICEFBlazorGuiService
{
    event Action<Player>? PlayerCEFBlazorGuiStarted;
    event Action<Player>? PlayerCEFBlazorGuiStopped;

    CEFGuiBlazorMode CEFGuiMode { get; internal set; }
    Action<IMessage>? MessageHandler { get; set; }
    Func<Player, string, string, Task>? RelayVoidAsyncInvoked { get; set; }
    Func<Player, string, string, Task<object?>>? RelayAsyncInvoked { get; set; }
    Action<Player>? RelayPlayerBrowserReady { get; set; }
    Action<Player>? RelayPlayerBlazorReady { get; set; }

    internal void HandlePlayerBrowserReady(Player player);

    void SetDevelopmentMode(Player player, bool isDevelopmentMode);
    void ToggleDevTools(Player player, bool isDevelopmentMode);
    void SetVisible(Player player, bool visible);
    void SetPath(Player player, string path, bool force, bool isAsync);
    internal void HandleInvokeVoidAsyncHandler(Player player, string identifier, string args);
    internal Task<object> HandleInvokeAsyncHandler(Player player, string identifier, string args);
}

using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.CEFBlazorGui;

public interface ICEFBlazorGuiService
{
    event Action<Player>? PlayerCEFBlazorGuiStarted;
    event Action<Player>? PlayerCEFBlazorGuiStopped;

    CEFGuiBlazorMode CEFGuiMode { get; internal set; }
    Action<IMessage>? MessageHandler { get; set; }
    Action<Player, string, string>? InvokeVoidAsyncInvoked { get; set; }
    Func<Player, string, string, Task<object>>? InvokeAsyncInvoked { get; set; }

    internal void HandleCEFBlazorGuiStart(Player player);
    internal void HandleCEFBlazorGuiStop(Player player);

    bool IsCEFBlazorGui(Player player);
    void SetDevelopmentMode(Player player, bool isDevelopmentMode);
    void ToggleDevTools(Player player, bool isDevelopmentMode);
    void SetVisible(Player player, bool visible);
    void SetPath(Player player, string path);
    internal void HandleInvokeVoidAsyncHandler(Player player, string identifier, string args);
    internal Task<object> HandleInvokeAsyncHandler(Player player, string identifier, string args);
}

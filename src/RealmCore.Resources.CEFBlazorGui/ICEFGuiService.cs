using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.CEFBlazorGui;

public interface ICEFBlazorGuiService
{
    event Action<Player>? PlayerCEFBlazorGuiStarted;
    event Action<Player>? PlayerCEFBlazorGuiStopped;
    event Action<Player, string, string>? InvokeVoidAsyncInvoked;

    CEFGuiBlazorMode CEFGuiMode { get; internal set; }
    Action<IMessage>? MessageHandler { get; set; }

    internal void HandleCEFBlazorGuiStart(Player player);
    internal void HandleCEFBlazorGuiStop(Player player);
    internal void HandleCEFInvokeVoidAsync(Player player, string identifier, string args);

    bool IsCEFBlazorGui(Player player);
    void SetDevelopmentMode(Player player, bool isDevelopmentMode);
    void ToggleDevTools(Player player, bool isDevelopmentMode);
    void SetVisible(Player player, bool visible);
    void SetPath(Player player, string path);
}

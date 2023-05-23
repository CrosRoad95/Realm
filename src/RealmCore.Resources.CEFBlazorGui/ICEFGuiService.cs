using SlipeServer.Server.Elements;

namespace RealmCore.Resources.CEFBlazorGui;

public interface ICEFBlazorGuiService
{
    event Action<Player>? PlayerCEFBlazorGuiStarted;
    event Action<Player>? PlayerCEFBlazorGuiStopped;

    internal void HandleCEFBlazorGuiStart(Player player);
    internal void HandleCEFBlazorGuiStop(Player player);

    bool IsCEFBlazorGui(Player player);
}

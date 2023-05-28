using RealmCore.Resources.Base.Interfaces;
using RealmCore.Resources.CEFBlazorGui.Messages;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.CEFBlazorGui;

internal sealed class CEFBlazorGuiService : ICEFBlazorGuiService
{
    public event Action<Player>? PlayerCEFBlazorGuiStarted;
    public event Action<Player>? PlayerCEFBlazorGuiStopped;
    private readonly HashSet<Player> _CEFBlazorGuiPlayers = new();
    public Action<IMessage>? MessageHandler { get; set; }

    public event Action<Player, string, string>? InvokeVoidAsyncInvoked;

    public CEFGuiBlazorMode CEFGuiMode { get; set; }

    public CEFBlazorGuiService(CEFGuiBlazorMode defaultCEFGuiBlazorMode, HttpDebugServer httpDebugServer)
    {
        CEFGuiMode = defaultCEFGuiBlazorMode;
        if (defaultCEFGuiBlazorMode == CEFGuiBlazorMode.Dev)
        {
            httpDebugServer.Start();
            httpDebugServer.InvokeVoidAsyncHandler = HandleCEFInvokeVoidAsync;
        }
    }

    public bool IsCEFBlazorGui(Player player) => _CEFBlazorGuiPlayers.Contains(player);

    public void HandleCEFBlazorGuiStart(Player player)
    {
        if (_CEFBlazorGuiPlayers.Add(player))
            PlayerCEFBlazorGuiStarted?.Invoke(player);
    }

    public void HandleCEFBlazorGuiStop(Player player)
    {
        if (_CEFBlazorGuiPlayers.Remove(player))
            PlayerCEFBlazorGuiStopped?.Invoke(player);
    }

    public void HandleCEFInvokeVoidAsync(Player player, string identifier, string args)
    {
        InvokeVoidAsyncInvoked?.Invoke(player, identifier, args);
    }

    public void SetDevelopmentMode(Player player, bool isDevelopmentMode)
    {
        MessageHandler?.Invoke(new SetDevelopmentModeMessage(player, isDevelopmentMode));
    }

    public void ToggleDevTools(Player player, bool enabled)
    {
        MessageHandler?.Invoke(new ToggleDevToolsMessage(player, enabled));
    }

    public void SetVisible(Player player, bool visible)
    {
        MessageHandler?.Invoke(new SetVisibleMessage(player, visible));
    }

    public void SetPath(Player player, string path)
    {
        MessageHandler?.Invoke(new SetPathMessage(player, path));
    }
}

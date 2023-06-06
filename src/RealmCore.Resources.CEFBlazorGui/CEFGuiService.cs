using RealmCore.Resources.Base.Interfaces;
using RealmCore.Resources.CEFBlazorGui.DebugServer;
using RealmCore.Resources.CEFBlazorGui.Messages;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.CEFBlazorGui;

internal sealed class CEFBlazorGuiService : ICEFBlazorGuiService
{
    public event Action<Player>? PlayerCEFBlazorGuiStarted;
    public event Action<Player>? PlayerCEFBlazorGuiStopped;
    private readonly HashSet<Player> _CEFBlazorGuiPlayers = new();
    private readonly BlazorDebugServer? _blazorDebugServer;
    private readonly IElementCollection _elementCollection;

    public Action<IMessage>? MessageHandler { get; set; }

    public Action<Player, string, string, string>? RelayVoidAsyncInvoked { get; set; }
    public Func<Player, string, string, string, Task<object>>? RelayAsyncInvoked { get; set; }
    public Action<Player>? RelayPlayerBrowserReady { get; set; }
    public Action<Player>? RelayPlayerBlazorReady { get; set; }

    public CEFGuiBlazorMode CEFGuiMode { get; set; }

    public CEFBlazorGuiService(CEFGuiBlazorMode defaultCEFGuiBlazorMode, IElementCollection elementCollection)
    {
        CEFGuiMode = defaultCEFGuiBlazorMode;
        _elementCollection = elementCollection;
        if (defaultCEFGuiBlazorMode == CEFGuiBlazorMode.Dev)
        {
            _blazorDebugServer = new()
            {
                InvokeAsyncHandler = HandleInvokeAsyncHandler,
                InvokeVoidAsyncHandler = HandleInvokeVoidAsyncHandler
            };
            Task.Run(_blazorDebugServer.Start);
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

    public void HandlePlayerBrowserReady(Player player)
    {
        RelayPlayerBrowserReady?.Invoke(player);
    }

    public void HandlePlayerBlazorReady(Player player)
    {
        RelayPlayerBlazorReady?.Invoke(player);
    }

    public void HandleInvokeVoidAsyncHandler(string kind, string identifier, string args)
    {
        RelayVoidAsyncInvoked?.Invoke(_elementCollection.GetByType<Player>().First(), kind, identifier, args);
    }

    public async Task<object> HandleInvokeAsyncHandler(string kind, string identifier, string args)
    {
        if (RelayAsyncInvoked != null)
            return await RelayAsyncInvoked(_elementCollection.GetByType<Player>().First(), kind, identifier, args);
        return null;
    }

    public void HandleInvokeVoidAsyncHandler(Player player, string kind, string identifier, string args)
    {
        switch(identifier)
        {
            case "_ready":
                RelayPlayerBlazorReady?.Invoke(player);
                break;
            default:
                RelayVoidAsyncInvoked?.Invoke(player, kind, identifier, args);
                break;
        }
    }

    public async Task<object> HandleInvokeAsyncHandler(Player player, string kind, string identifier, string args)
    {
        if (RelayAsyncInvoked != null)
            return await RelayAsyncInvoked(player, kind, identifier, args);
        return null;
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

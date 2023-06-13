using Microsoft.Extensions.Options;
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
    private BlazorDebugServer? _blazorDebugServer;
    private readonly IElementCollection _elementCollection;

    public Action<IMessage>? MessageHandler { get; set; }

    public Func<Player, string, string, Task>? RelayVoidAsyncInvoked { get; set; }
    public Func<Player, string, string, Task<object>>? RelayAsyncInvoked { get; set; }
    public Action<Player>? RelayPlayerBrowserReady { get; set; }
    public Action<Player>? RelayPlayerBlazorReady { get; set; }

    public CEFGuiBlazorMode CEFGuiMode { get; set; }

    public CEFBlazorGuiService(IOptions<BlazorOptions> blazorOptions, IElementCollection elementCollection)
    {
        CEFGuiMode = blazorOptions.Value.Mode;
        _elementCollection = elementCollection;
    }

    public void StartDebugServer()
    {
        _blazorDebugServer = new()
        {
            InvokeAsyncHandler = HandleInvokeAsyncHandler,
            InvokeVoidAsyncHandler = HandleInvokeVoidAsyncHandler
        };
        Task.Run(_blazorDebugServer.Start);
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

    public Task HandleInvokeVoidAsyncHandler(string identifier, string args)
    {
        return RelayVoidAsyncInvoked?.Invoke(_elementCollection.GetByType<Player>().First(), identifier, args);
    }

    public async Task<object> HandleInvokeAsyncHandler(string identifier, string args)
    {
        if (RelayAsyncInvoked != null)
            return await RelayAsyncInvoked(_elementCollection.GetByType<Player>().First(), identifier, args);
        return null;
    }

    public void HandleInvokeVoidAsyncHandler(Player player, string identifier, string args)
    {
        switch(identifier)
        {
            case "_ready":
                RelayPlayerBlazorReady?.Invoke(player);
                break;
            default:
                RelayVoidAsyncInvoked?.Invoke(player, identifier, args);
                break;
        }
    }

    public async Task<object> HandleInvokeAsyncHandler(Player player, string identifier, string args)
    {
        if (RelayAsyncInvoked != null)
            return await RelayAsyncInvoked(player, identifier, args);
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

    public void SetPath(Player player, string path, bool force)
    {
        MessageHandler?.Invoke(new SetPathMessage(player, path, force));
    }
}

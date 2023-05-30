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

    public Action<Player, string, string>? InvokeVoidAsyncInvoked { get; set; }
    public Func<Player, string, string, Task<object>>? InvokeAsyncInvoked { get; set; }

    public CEFGuiBlazorMode CEFGuiMode { get; set; }

    public CEFBlazorGuiService(CEFGuiBlazorMode defaultCEFGuiBlazorMode, IElementCollection elementCollection)
    {
        CEFGuiMode = defaultCEFGuiBlazorMode;
        _elementCollection = elementCollection;
        if (defaultCEFGuiBlazorMode == CEFGuiBlazorMode.Dev)
        {
            _blazorDebugServer = new();
            _blazorDebugServer.InvokeAsyncHandler = HandleInvokeAsyncHandler;
            _blazorDebugServer.InvokeVoidAsyncHandler = HandleInvokeVoidAsyncHandler;
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

    public void HandleInvokeVoidAsyncHandler(string identifier, string args)
    {
        InvokeVoidAsyncInvoked?.Invoke(_elementCollection.GetByType<Player>().First(), identifier, args);
    }

    public async Task<object> HandleInvokeAsyncHandler(string identifier, string args)
    {
        if (InvokeAsyncInvoked != null)
            return await InvokeAsyncInvoked(_elementCollection.GetByType<Player>().First(), identifier, args);
        return null;
    }

    public void HandleInvokeVoidAsyncHandler(Player player, string identifier, string args)
    {
        InvokeVoidAsyncInvoked?.Invoke(player, identifier, args);
    }

    public async Task<object> HandleInvokeAsyncHandler(Player player, string identifier, string args)
    {
        if (InvokeAsyncInvoked != null)
            return await InvokeAsyncInvoked(player, identifier, args);
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

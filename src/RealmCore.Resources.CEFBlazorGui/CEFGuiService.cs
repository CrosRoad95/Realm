using Microsoft.Extensions.Logging;
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
    private BlazorDebugServer? _blazorDebugServer;
    private readonly IElementCollection _elementCollection;
    private readonly ILogger<CEFBlazorGuiService> _logger;

    public Action<IMessage>? MessageHandler { get; set; }

    public Func<Player, string, string, Task>? RelayVoidAsyncInvoked { get; set; }
    public Func<Player, string, string, Task<object>>? RelayAsyncInvoked { get; set; }
    public Action<Player>? RelayPlayerBrowserReady { get; set; }
    public Action<Player>? RelayPlayerBlazorReady { get; set; }

    public CEFGuiBlazorMode CEFGuiMode { get; set; }

    public CEFBlazorGuiService(IOptions<BlazorOptions> blazorOptions, IElementCollection elementCollection, ILogger<CEFBlazorGuiService> logger)
    {
        CEFGuiMode = blazorOptions.Value.Mode;
        _elementCollection = elementCollection;
        _logger = logger;
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
        try
        {
            return RelayVoidAsyncInvoked?.Invoke(_elementCollection.GetByType<Player>().First(), identifier, args);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to relay invokeVoidAsync");
        }

        return Task.CompletedTask;
    }

    public async Task<object?> HandleInvokeAsyncHandler(string identifier, string args)
    {
        if (RelayAsyncInvoked != null)
            return await RelayAsyncInvoked(_elementCollection.GetByType<Player>().First(), identifier, args);
        return null;
    }

    public void HandleInvokeVoidAsyncHandler(Player player, string identifier, string args)
    {
        try
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
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to relay invokeVoidAsync");
        }
    }

    public async Task<object?> HandleInvokeAsyncHandler(Player player, string identifier, string args)
    {
        try
        {
            if (RelayAsyncInvoked != null)
                return await RelayAsyncInvoked(player, identifier, args);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to relay invokeAsync");
        }
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

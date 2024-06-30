using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;
using System.Numerics;

namespace RealmCore.Resources.Browser;

internal sealed class BrowserLogic
{
    private readonly IBrowserService _browserService;
    private readonly ILogger<BrowserLogic> _logger;
    private readonly ILuaEventHub<IBrowserEventHub> _luaEventHub;
    private readonly IOptions<BrowserOptions> _browserOptions;
    private readonly BrowserResource _resource;

    public BrowserLogic(MtaServer server, LuaEventService luaEventService, IBrowserService BrowserService,
        ILogger<BrowserLogic> logger, ILuaEventHub<IBrowserEventHub> luaEventHub, IOptions<BrowserOptions> browserOptions)
    {
        luaEventService.AddEventHandler("internalBrowserCreated", HandleBrowserCreated);
        luaEventService.AddEventHandler("internalBrowserDocumentReady", HandleBrowserDocumentReady);
        _browserService = BrowserService;
        _logger = logger;
        _luaEventHub = luaEventHub;
        _browserOptions = browserOptions;
        _resource = server.GetAdditionalResource<BrowserResource>();
        server.PlayerJoined += HandlePlayerJoin;
        BrowserService.MessageHandler = HandleMessage;
    }

    private async void HandlePlayerJoin(Player player)
    {
        try
        {
            await _resource.StartForAsync(player);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to start {resourceName} resource for player: {playerName}, serial: {playerSerial}", _resource.Name, player.Name, player.Client.Serial ?? "<no serial>");
        }
    }

    private void HandleMessage(IMessage message)
    {
        // TODO: add loggers
        switch (message)
        {
            case SetVisibleMessage setVisibleMessage:
                var enabled = setVisibleMessage.Enabled;
                _luaEventHub.Invoke(setVisibleMessage.Player, x => x.SetVisible(enabled));
                break;
            case ToggleDevToolsMessage toggleDevToolsMessages:
                _luaEventHub.Invoke(toggleDevToolsMessages.Player, x => x.ToggleDevTools(toggleDevToolsMessages.Enabled));
                break;
            case SetPathMessage setPathMessage:
                _luaEventHub.Invoke(setPathMessage.Player, x => x.SetPath(setPathMessage.Path));
                break;
            case LoadBrowser loadBrowser:
                {
                    var width = (int)loadBrowser.Size.X;
                    var height = (int)loadBrowser.Size.Y;
                    _luaEventHub.Invoke(loadBrowser.Player, x => x.Load(width, height, loadBrowser.RemoteUrl, loadBrowser.RequestWhitelistUrl));
                }
                break;
        }
    }

    private void HandleBrowserDocumentReady(LuaEvent luaEvent)
    {
        _browserService.RelayBrowserReady(luaEvent.Player);
    }

    private void HandleBrowserCreated(LuaEvent luaEvent)
    {
        _browserService.RelayBrowserStarted(luaEvent.Player);
    }
}

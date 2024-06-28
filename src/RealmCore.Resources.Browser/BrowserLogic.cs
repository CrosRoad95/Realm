using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

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
            var width = _browserOptions.Value.BrowserWidth;
            var height = _browserOptions.Value.BrowserHeight;
            var remoteUrl = _browserOptions.Value.BaseRemoteUrl;
            var requestWhitelistUrl = _browserOptions.Value.RequestWhitelistUrl;
            _luaEventHub.Invoke(player, x => x.Load(width, height, remoteUrl, requestWhitelistUrl));
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

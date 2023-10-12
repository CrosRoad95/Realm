using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Services;

namespace RealmCore.Resources.CEFBlazorGui;

internal class CEFBlazorGuiLogic
{
    private readonly ICEFBlazorGuiService _CEFBlazorGuiService;
    private readonly ILogger<CEFBlazorGuiLogic> _logger;
    private readonly ILuaEventHub<ICEFBlazorGuiEventHub> _luaEventHub;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly IOptions<BrowserOptions> _blazorOptions;
    private readonly CEFBlazorGuiResource _resource;

    public CEFBlazorGuiLogic(MtaServer mtaServer, LuaEventService luaEventService, ICEFBlazorGuiService CEFBlazorGuiService,
        ILogger<CEFBlazorGuiLogic> logger, ILuaEventHub<ICEFBlazorGuiEventHub> luaEventHub, FromLuaValueMapper fromLuaValueMapper, IOptions<BrowserOptions> blazorOptions)
    {
        //luaEventService.AddEventHandler("internalBrowserCreated", HandleBrowserCreated);
        luaEventService.AddEventHandler("internalBrowserDocumentReady", HandleBrowserDocumentReady);
        _CEFBlazorGuiService = CEFBlazorGuiService;
        _logger = logger;
        _luaEventHub = luaEventHub;
        _fromLuaValueMapper = fromLuaValueMapper;
        _blazorOptions = blazorOptions;
        _resource = mtaServer.GetAdditionalResource<CEFBlazorGuiResource>();
        mtaServer.PlayerJoined += HandlePlayerJoin;
        CEFBlazorGuiService.MessageHandler = HandleMessage;
    }

    private async void HandlePlayerJoin(Player player)
    {
        try
        {
            await _resource.StartForAsync(player);
            var mode = _blazorOptions.Value.Mode.ToString().ToLower();
            var width = _blazorOptions.Value.BrowserWidth;
            var height = _blazorOptions.Value.BrowserHeight;
            var remoteUrl = _blazorOptions.Value.BaseRemoteUrl;
            var requestWhitelistUrl = _blazorOptions.Value.RequestWhitelistUrl;
            _luaEventHub.Invoke(player, x => x.Load(mode, width, height, remoteUrl, requestWhitelistUrl));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to start {resourceName} resource for player: {playerName}, serial: {playerSerial}", _resource.Name, player.Name, player.Client.Serial);
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
                _luaEventHub.Invoke(setPathMessage.Player, x => x.SetPath(setPathMessage.Path, setPathMessage.Force, setPathMessage.IsAsync));
                break;
            case SetRemotePathMessage setRemotePathMessage:
                _luaEventHub.Invoke(setRemotePathMessage.Player, x => x.SetRemotePath(setRemotePathMessage.Path));
                break;
        }
    }

    private void HandleBrowserDocumentReady(LuaEvent luaEvent)
    {
        _CEFBlazorGuiService.HandlePlayerBrowserReady(luaEvent.Player);
    }
}

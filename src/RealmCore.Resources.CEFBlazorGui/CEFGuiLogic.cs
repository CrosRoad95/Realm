using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RealmCore.Resources.Base;
using RealmCore.Resources.Base.Interfaces;
using RealmCore.Resources.CEFBlazorGui.Messages;
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
    private readonly CEFBlazorGuiResource _resource;

    public CEFBlazorGuiLogic(MtaServer mtaServer, LuaEventService luaEventService, ICEFBlazorGuiService CEFBlazorGuiService,
        ILogger<CEFBlazorGuiLogic> logger, ILuaEventHub<ICEFBlazorGuiEventHub> luaEventHub, FromLuaValueMapper fromLuaValueMapper)
    {
        luaEventService.AddEventHandler("internalCEFBlazorGuiStart", HandleCEFBlazorGuiStart);
        luaEventService.AddEventHandler("internalCEFBlazorGuiStop", HandleCEFBlazorGuiStop);
        luaEventService.AddEventHandler("cefInvokeVoidAsync", HandleCEFInvokeVoidAsync);
        luaEventService.AddEventHandler("cefInvokeAsync", HandleCEFInvokeAsync);
        luaEventService.AddEventHandler("internalBrowserCreated", HandleBrowserCreated);
        _CEFBlazorGuiService = CEFBlazorGuiService;
        _logger = logger;
        _luaEventHub = luaEventHub;
        _fromLuaValueMapper = fromLuaValueMapper;
        _resource = mtaServer.GetAdditionalResource<CEFBlazorGuiResource>();
        mtaServer.PlayerJoined += HandlePlayerJoin;
        CEFBlazorGuiService.MessageHandler = HandleMessage;
    }

    private async void HandlePlayerJoin(Player player)
    {
        try
        {
            await  _resource.StartForAsync(player);
            _luaEventHub.Invoke(player, x => x.SetDevelopmentMode(true));
            var mode = _CEFBlazorGuiService.CEFGuiMode.ToString().ToLower();
            _luaEventHub.Invoke(player, x => x.Load(mode, 1024, 768));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to start {resourceName} resource for player: {playerName}, serial: {playerSerial}", _resource.Name, player.Name, player.Client.Serial);
        }
    }

    private void HandleCEFBlazorGuiStart(LuaEvent luaEvent)
    {
        _CEFBlazorGuiService.HandleCEFBlazorGuiStart(luaEvent.Player);
    }

    private void HandleCEFBlazorGuiStop(LuaEvent luaEvent)
    {
        _CEFBlazorGuiService.HandleCEFBlazorGuiStop(luaEvent.Player);
    }

    private void HandleMessage(IMessage message)
    {
        // TODO: add loggers
        switch (message)
        {
            case SetDevelopmentModeMessage setDevelopmentModeMessage:
                _luaEventHub.Invoke(setDevelopmentModeMessage.Player, x => x.SetDevelopmentMode(setDevelopmentModeMessage.Enabled));
                break;
            case SetVisibleMessage setVisibleMessage:
                _luaEventHub.Invoke(setVisibleMessage.Player, x => x.SetVisible(setVisibleMessage.Enabled));
                break;
            case ToggleDevToolsMessage toggleDevToolsMessages:
                _luaEventHub.Invoke(toggleDevToolsMessages.Player, x => x.ToggleDevTools(toggleDevToolsMessages.Enabled));
                break;
            case SetPathMessage setPathMessage:
                _luaEventHub.Invoke(setPathMessage.Player, x => x.SetPath(setPathMessage.Path));
                break;
        }
    }


    private void HandleCEFInvokeVoidAsync(LuaEvent luaEvent)
    {
        var (identifier, args) = luaEvent.Read<string, string>(_fromLuaValueMapper);
        _CEFBlazorGuiService.HandleInvokeVoidAsyncHandler(luaEvent.Player, identifier, args);
    }

    private async void HandleCEFInvokeAsync(LuaEvent luaEvent)
    {
        try
        {
            var (identifier, promiseId, args) = luaEvent.Read<string, string, string>(_fromLuaValueMapper);
            try
            {
                var value = await _CEFBlazorGuiService.HandleInvokeAsyncHandler(luaEvent.Player, identifier, args);
                var data = JsonConvert.SerializeObject(value);
                _luaEventHub.Invoke(luaEvent.Player, x => x.InvokeAsyncSuccess(promiseId, data));
            }
            catch(Exception ex)
            {
                _luaEventHub.Invoke(luaEvent.Player, x => x.InvokeAsyncError(promiseId));
                throw;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to handle cef invoke async");
        }
    }

    private void HandleBrowserCreated(LuaEvent luaEvent)
    {
        _CEFBlazorGuiService.HandlePlayerBrowserReady(luaEvent.Player);
    }
}

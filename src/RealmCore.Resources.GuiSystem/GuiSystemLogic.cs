using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace RealmCore.Resources.GuiSystem;

internal class GuiSystemLogic
{
    private readonly IGuiSystemService _GuiSystemService;
    private readonly ILogger<GuiSystemLogic> _logger;
    private readonly GuiSystemResource _resource;

    public GuiSystemLogic(MtaServer server, LuaEventService luaEventService, IGuiSystemService GuiSystemService, ILogger<GuiSystemLogic> logger)
    {
        luaEventService.AddEventHandler("internalSubmitForm", HandleInternalSubmitForm);
        luaEventService.AddEventHandler("internalActionExecuted", HandleInternalActionExecuted);
        _GuiSystemService = GuiSystemService;
        _logger = logger;
        _resource = server.GetAdditionalResource<GuiSystemResource>();
        server.PlayerJoined += HandlePlayerJoin;
    }

    public void HandleInternalSubmitForm(LuaEvent luaEvent)
    {
        _GuiSystemService.HandleInternalSubmitForm(luaEvent);
    }

    public void HandleInternalActionExecuted(LuaEvent luaEvent)
    {
        _GuiSystemService.HandleInternalActionExecuted(luaEvent);
    }

    private async void HandlePlayerJoin(Player player)
    {

        try
        {
            await _resource.StartForAsync(player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start {resourceName} resource for player: {playerName}, serial: {playerSerial}", _resource.Name, player.Name, player.Client.Serial);
        }
    }
}

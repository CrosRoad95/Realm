using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace Realm.Resources.AgnosticGuiSystem;

internal class AgnosticGuiSystemLogic
{
    private readonly AgnosticGuiSystemService _agnosticGuiSystemService;
    private readonly AgnosticGuiSystemResource _resource;

    public AgnosticGuiSystemLogic(MtaServer server, LuaEventService luaEventService, AgnosticGuiSystemService agnosticGuiSystemService)
    {
        luaEventService.AddEventHandler("internalSubmitForm", HandleInternalSubmitForm);
        luaEventService.AddEventHandler("internalRequestGuiClose", HandleInternalRequestGuiClose);
        luaEventService.AddEventHandler("internalNavigateToGui", HandleInternalNavigateToGui);
        _agnosticGuiSystemService = agnosticGuiSystemService;
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<AgnosticGuiSystemResource>();
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    public void HandleInternalSubmitForm(LuaEvent luaEvent)
    {
        _agnosticGuiSystemService.HandleInternalSubmitForm(luaEvent);
    }

    public void HandleInternalRequestGuiClose(LuaEvent luaEvent)
    {
        _agnosticGuiSystemService.HandleInternalRequestGuiClose(luaEvent);
    }

    public void HandleInternalNavigateToGui(LuaEvent luaEvent)
    {
        _agnosticGuiSystemService.HandleInternalNavigateToGui(luaEvent);
    }
}

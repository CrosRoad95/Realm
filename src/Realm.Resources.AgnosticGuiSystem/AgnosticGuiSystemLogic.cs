using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace Realm.Resources.AgnosticGuiSystem;

internal class AgnosticGuiSystemLogic
{
    private readonly IAgnosticGuiSystemService _agnosticGuiSystemService;
    private readonly AgnosticGuiSystemResource _resource;

    public AgnosticGuiSystemLogic(MtaServer server, LuaEventService luaEventService, IAgnosticGuiSystemService agnosticGuiSystemService)
    {
        luaEventService.AddEventHandler("internalSubmitForm", HandleInternalSubmitForm);
        luaEventService.AddEventHandler("internalActionExecuted", HandleInternalActionExecuted);
        _agnosticGuiSystemService = agnosticGuiSystemService;

        _resource = server.GetAdditionalResource<AgnosticGuiSystemResource>();
        server.PlayerJoined += HandlePlayerJoin;
    }

    public void HandleInternalSubmitForm(LuaEvent luaEvent)
    {
        _agnosticGuiSystemService.HandleInternalSubmitForm(luaEvent);
    }
    
    public void HandleInternalActionExecuted(LuaEvent luaEvent)
    {
        _agnosticGuiSystemService.HandleInternalActionExecuted(luaEvent);
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}

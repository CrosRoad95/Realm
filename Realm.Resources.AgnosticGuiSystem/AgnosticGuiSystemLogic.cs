using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace Realm.Resources.AgnosticGuiSystem;

internal class AgnosticGuiSystemLogic
{
    private readonly AgnosticGuiSystemService _agnosticGuiSystemService;

    public AgnosticGuiSystemLogic(LuaEventService luaEventService, AgnosticGuiSystemService agnosticGuiSystemService)
    {
        luaEventService.AddEventHandler("internalSubmitForm", HandleInternalSubmitForm);
        luaEventService.AddEventHandler("internalRequestGuiClose", HandleInternalRequestGuiClose);
        luaEventService.AddEventHandler("internalNavigateToGui", HandleInternalNavigateToGui);
        _agnosticGuiSystemService = agnosticGuiSystemService;
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

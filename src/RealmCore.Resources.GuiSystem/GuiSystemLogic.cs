using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace RealmCore.Resources.GuiSystem;

internal class GuiSystemLogic
{
    private readonly IGuiSystemService _GuiSystemService;
    private readonly GuiSystemResource _resource;

    public GuiSystemLogic(MtaServer server, LuaEventService luaEventService, IGuiSystemService GuiSystemService)
    {
        luaEventService.AddEventHandler("internalSubmitForm", HandleInternalSubmitForm);
        luaEventService.AddEventHandler("internalActionExecuted", HandleInternalActionExecuted);
        _GuiSystemService = GuiSystemService;

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

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }
}

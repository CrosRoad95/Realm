using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace RealmCore.Resources.CEFBlazorGui;

internal class CEFBlazorGuiLogic
{
    private readonly ICEFBlazorGuiService _CEFBlazorGuiService;
    private readonly CEFBlazorGuiResource _resource;

    public CEFBlazorGuiLogic(MtaServer mtaServer, LuaEventService luaEventService, ICEFBlazorGuiService CEFBlazorGuiService)
    {
        luaEventService.AddEventHandler("internalCEFBlazorGuiStart", HandleCEFBlazorGuiStart);
        luaEventService.AddEventHandler("internalCEFBlazorGuiStop", HandleCEFBlazorGuiStop);
        _CEFBlazorGuiService = CEFBlazorGuiService;
        _resource = mtaServer.GetAdditionalResource<CEFBlazorGuiResource>();

        mtaServer.PlayerJoined += HandlePlayerJoin;
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void HandleCEFBlazorGuiStart(LuaEvent luaEvent)
    {
        _CEFBlazorGuiService.HandleCEFBlazorGuiStart(luaEvent.Player);
    }

    private void HandleCEFBlazorGuiStop(LuaEvent luaEvent)
    {
        _CEFBlazorGuiService.HandleCEFBlazorGuiStop(luaEvent.Player);
    }
}

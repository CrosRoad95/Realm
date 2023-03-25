using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace Realm.Resources.AFK;

internal class AFKLogic
{
    private readonly IAFKService _AFKService;
    private readonly AFKResource _resource;

    public AFKLogic(MtaServer mtaServer, LuaEventService luaEventService, IAFKService AFKService)
    {
        luaEventService.AddEventHandler("internalAFKStart", HandleAFKStart);
        luaEventService.AddEventHandler("internalAFKStop", HandleAFKStop);
        _AFKService = AFKService;
        _resource = mtaServer.GetAdditionalResource<AFKResource>();

        mtaServer.PlayerJoined += HandlePlayerJoin;
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void HandleAFKStart(LuaEvent luaEvent)
    {
        _AFKService.HandleAFKStart(luaEvent.Player);
    }

    private void HandleAFKStop(LuaEvent luaEvent)
    {
        _AFKService.HandleAFKStop(luaEvent.Player);
    }
}

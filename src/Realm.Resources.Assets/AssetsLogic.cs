using SlipeServer.Server.Elements;
using SlipeServer.Server;
using SlipeServer.Server.Services;
using SlipeServer.Server.Events;
using SlipeServer.Packets.Definitions.Lua;

namespace Realm.Resources.Assets;

internal class AssetsLogic
{
    private readonly AssetsResource _resource;
    private readonly AssetsRegistry _assetsRegistry;
    private readonly IAssetsService _assetsService;
    private readonly LuaEventService _luaEventService;
    public AssetsLogic(MtaServer server, AssetsRegistry assetsRegistry, IAssetsService assetsService, LuaEventService luaEventService)
    {
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<AssetsResource>();
        _assetsRegistry = assetsRegistry;
        _assetsService = assetsService;
        _luaEventService = luaEventService;
        luaEventService.AddEventHandler("internalRequestAssets", HandleInternalRequestAssets);
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void HandleInternalRequestAssets(LuaEvent luaEvent)
    {
        var luaValue = _assetsRegistry.Assets.ToDictionary(x => new LuaValue(x.Key), x => _assetsService.Map(x.Value));
        _luaEventService.TriggerEventFor(luaEvent.Player, "internalResponseRequestAsset", luaEvent.Player, new LuaValue(luaValue));
    }

}

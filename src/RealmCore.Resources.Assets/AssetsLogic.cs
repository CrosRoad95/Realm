using SlipeServer.Server.Elements;
using SlipeServer.Server;
using SlipeServer.Server.Services;
using SlipeServer.Server.Events;
using SlipeServer.Packets.Definitions.Lua;

namespace RealmCore.Resources.Assets;

internal class AssetsLogic
{
    private readonly AssetsResource _resource;
    private readonly AssetsCollection _assetsCollection;
    private readonly IAssetsService _assetsService;
    private readonly LuaEventService _luaEventService;
    public AssetsLogic(MtaServer server, AssetsCollection assetsCollection, IAssetsService assetsService, LuaEventService luaEventService)
    {
        _assetsCollection = assetsCollection;
        _assetsService = assetsService;
        _luaEventService = luaEventService;
        luaEventService.AddEventHandler("internalRequestAssets", HandleInternalRequestAssets);
        _resource = server.GetAdditionalResource<AssetsResource>();

        _assetsService.ReplaceModelForPlayer = HandleReplaceModelForPlayer;
        _assetsService.RestoreModelForPlayer = HandleRestoreModelForPlayer;

        server.PlayerJoined += HandlePlayerJoin;
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void HandleInternalRequestAssets(LuaEvent luaEvent)
    {
        var luaValue = _assetsCollection.Assets.ToDictionary(x => new LuaValue(x.Key), x => _assetsService.Map(x.Value));
        _luaEventService.TriggerEventFor(luaEvent.Player, "internalResponseRequestAsset", luaEvent.Player, new LuaValue(luaValue));
    }

    private void HandleReplaceModelForPlayer(Player player, Stream dff, Stream col, ushort model)
    {
        var dffString = Convert.ToBase64String(Utilities.ReadFully(dff));
        var colString = Convert.ToBase64String(Utilities.ReadFully(col));
        _luaEventService.TriggerEventFor(player, "internalReplaceModel", player, dffString, colString, (int)model);
    }

    private void HandleRestoreModelForPlayer(Player player,ushort model)
    {
        _luaEventService.TriggerEventFor(player, "internalRestoreModel", player, (int)model);
    }
}

using SlipeServer.Server.Elements;
using SlipeServer.Server;
using SlipeServer.Server.Services;
using SlipeServer.Server.Events;
using SlipeServer.Packets.Definitions.Lua;
using Microsoft.Extensions.Logging;
using RealmCore.Resources.Assets.Interfaces;

namespace RealmCore.Resources.Assets;

internal class AssetsLogic
{
    private readonly AssetsResource _resource;
    private readonly AssetsRegistry _assetsRegistry;
    private readonly IAssetsService _assetsService;
    private readonly LuaEventService _luaEventService;
    public AssetsLogic(MtaServer server, AssetsRegistry assetsRegistry, IAssetsService assetsService, LuaEventService luaEventService, ILogger<AssetsLogic> logger, IEnumerable<IServerAssetsProvider> serverAssetsProviders)
    {
        server.PlayerJoined += HandlePlayerJoin;

        _assetsRegistry = assetsRegistry;
        _assetsService = assetsService;
        _luaEventService = luaEventService;
        luaEventService.AddEventHandler("internalRequestAssets", HandleInternalRequestAssets);

        _assetsService.ReplaceModelForPlayer = HandleReplaceModelForPlayer;
        _assetsService.RestoreModelForPlayer = HandleRestoreModelForPlayer;
        _resource = new AssetsResource(server, serverAssetsProviders);
        server.AddAdditionalResource(_resource, _resource.AdditionalFiles);
        logger.LogInformation("Loaded {count} assets of total size: {sizeInMB:N4}MB", _resource.AdditionalFiles.Count, _resource.ContentSize / 1024.0f / 1024.0f);
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

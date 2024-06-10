using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using SlipeServer.Server.Events;
using SlipeServer.Packets.Definitions.Lua;
using Microsoft.Extensions.Logging;
using SlipeServer.Server.Enums;
using Newtonsoft.Json;

namespace RealmCore.Resources.Assets;

internal record struct ReplaceModel(ushort model, string dff, string col);

internal class AssetsLogic
{
    private readonly AssetsResource _resource;
    private readonly AssetsCollection _assetsCollection;
    private readonly IAssetsService _assetsService;
    private readonly LuaEventService _luaEventService;
    private readonly ILogger<AssetsLogic> _logger;

    private readonly List<ReplaceModel> _replaceModels = [];

    public AssetsLogic(MtaServer server, AssetsCollection assetsCollection, IAssetsService assetsService, LuaEventService luaEventService, ILogger<AssetsLogic> logger)
    {
        _assetsCollection = assetsCollection;
        _assetsService = assetsService;
        _luaEventService = luaEventService;
        _logger = logger;
        _resource = server.GetAdditionalResource<AssetsResource>();

        _assetsService.ModelReplaced = HandleModelReplaced;
        _assetsService.ReplaceModelForPlayer = HandleReplaceModelForPlayer;
        _assetsService.RestoreModelForPlayer = HandleRestoreModelForPlayer;

        server.PlayerJoined += HandlePlayerJoin;
    }

    private async void HandlePlayerJoin(Player player)
    {
        try
        {
            await _resource.StartForAsync(player);
            SendAssetsList(player);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to start assets resource");
        }
    }

    private void SendAssetsList(Player player)
    {
        var replacedModels = JsonConvert.SerializeObject(_replaceModels);
        var luaValue = _assetsCollection.Assets.ToDictionary(x => new LuaValue(x.Key), x => _assetsService.Map(x.Value));
        _luaEventService.TriggerEventFor(player, "internalSetAssetsList", player, new LuaValue(luaValue), replacedModels);
    }

    private void HandleInternalRequestAssets(LuaEvent luaEvent)
    {
        SendAssetsList(luaEvent.Player);
    }

    private void HandleModelReplaced(ObjectModel objectModel, string dffName, string colName)
    {
        _replaceModels.Add(new ReplaceModel((ushort)objectModel, dffName, colName));
    }

    private void HandleReplaceModelForPlayer(Player player, ObjectModel objectModel, Stream dff, Stream col)
    {
        var dffString = Convert.ToBase64String(dff.ToArray());
        var colString = Convert.ToBase64String(col.ToArray());
        _luaEventService.TriggerEventFor(player, "internalReplaceModel", player, dffString, colString, (int)objectModel);
    }

    private void HandleRestoreModelForPlayer(Player player, ObjectModel model)
    {
        _luaEventService.TriggerEventFor(player, "internalRestoreModel", player, (int)model);
    }
}

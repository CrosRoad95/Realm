using SlipeServer.Server.Services;
using SlipeServer.Server.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RealmCore.Resources.Assets;

internal class AssetsLogic
{
    private readonly MtaServer _mtaServer;
    private readonly AssetsCollection _assetsCollection;
    private readonly IAssetsService _assetsService;
    private readonly LuaEventService _luaEventService;
    private readonly ILogger<AssetsLogic> _logger;

    private readonly List<ReplacedModel> _replacedModels = [];

    public AssetsLogic(MtaServer mtaServer, AssetsCollection assetsCollection, IAssetsService assetsService, LuaEventService luaEventService, ILogger<AssetsLogic> logger)
    {
        _mtaServer = mtaServer;
        _assetsCollection = assetsCollection;
        _assetsService = assetsService;
        _luaEventService = luaEventService;
        _logger = logger;

        _assetsService.ModelReplaced = HandleModelReplaced;
        _assetsService.ReplaceModelForPlayer = HandleReplaceModelForPlayer;
        _assetsService.RestoreModelForPlayer = HandleRestoreModelForPlayer;

        mtaServer.PlayerJoined += HandlePlayerJoin;
    }

    private async void HandlePlayerJoin(Player player)
    {
        try
        {
            var resource = _mtaServer.GetAdditionalResource<AssetsResource>();
            await resource.StartForAsync(player);
            SendAssetsList(player);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to start assets resource");
        }
    }

    private void SendAssetsList(Player player)
    {
        var replacedModels = JsonConvert.SerializeObject(_replacedModels);
        var luaValue = _assetsCollection.Assets.ToDictionary(x => new LuaValue(x.Key), x => _assetsService.Map(x.Value));
        _luaEventService.TriggerEventFor(player, "internalSetAssetsList", player, new LuaValue(luaValue), replacedModels);
    }

    private void HandleInternalRequestAssets(LuaEvent luaEvent)
    {
        SendAssetsList(luaEvent.Player);
    }

    private void HandleModelReplaced(ReplacedModel replacedModel)
    {
        _replacedModels.Add(replacedModel);
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

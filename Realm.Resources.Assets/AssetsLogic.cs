using SlipeServer.Server.Elements;
using SlipeServer.Server;
using SlipeServer.Server.Services;
using SlipeServer.Server.Events;
using SlipeServer.Packets.Definitions.Lua;
using Newtonsoft.Json;

namespace Realm.Resources.Assets;

internal class AssetsLogic
{
    private readonly AssetsResource _resource;
    private readonly AssetsService _assetsService;
    private readonly LuaEventService _luaEventService;
    private readonly Dictionary<string, (string, string[])> _assetsRegistry = new();
    private readonly Dictionary<string, (string, string[])> _assetsChecksums = new();
    public AssetsLogic(MtaServer server, AssetsService assetsService, LuaEventService luaEventService)
    {
        server.PlayerJoined += HandlePlayerJoin;

        _resource = server.GetAdditionalResource<AssetsResource>();
        _assetsService = assetsService;
        _luaEventService = luaEventService;
        _assetsService.ModelAdded += AssetsService_ModelAdded;
        luaEventService.AddEventHandler("internalRequestChecksums", HandleRequestChecksums);
        luaEventService.AddEventHandler("internalRequestAsset", HandleRequestAsset);
    }

    private void AssetsService_ModelAdded(string name, IModel model)
    {
        _assetsRegistry[name] = ("model", new string[] { Convert.ToBase64String(model.Dff), Convert.ToBase64String(model.Col) });
        _assetsChecksums[name] = ("model", new string[] { GetMD5checksum(model.Dff), GetMD5checksum(model.Col) });
    }

    public string GetMD5checksum(byte[] inputData)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        md5.TransformFinalBlock(inputData, 0, inputData.Length);
        return BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
    }

    private void HandlePlayerJoin(Player player)
    {
        _resource.StartFor(player);
    }

    private void HandleRequestChecksums(LuaEvent luaEvent)
    {
        _luaEventService.TriggerEventFor(luaEvent.Player, "internalResponseRequestChecksums", luaEvent.Player, JsonConvert.SerializeObject(_assetsChecksums));
    }

    private void HandleRequestAsset(LuaEvent luaEvent)
    {
        string name = ((string)luaEvent.Parameters[0]);
        var asset = _assetsRegistry[name];
        _luaEventService.TriggerEventFor(luaEvent.Player, "internalResponseRequestAsset", luaEvent.Player, name, JsonConvert.SerializeObject(asset));
    }

}

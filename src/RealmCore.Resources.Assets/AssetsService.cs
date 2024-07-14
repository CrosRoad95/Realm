using RealmCore.Resources.Assets.Classes;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;

namespace RealmCore.Resources.Assets;

public record struct ReplacedModel(ushort model, string modelAsset, string? collisionAsset, string textureAsset);

public interface IAssetsService
{
    IReadOnlyDictionary<ObjectModel, ReplacedModel> ReplacedModels { get; }
    internal Action<ReplacedModel>? ModelReplaced { get; set; }
    internal Action<Player, ObjectModel, Stream, Stream>? ReplaceModelForPlayer { get; set; }
    internal Action<Player, ObjectModel>? RestoreModelForPlayer { get; set; }

    LuaValue Map(IAsset asset);
    void ReplaceModel(ObjectModel objectModel, string modelAsset, string collisionAsset, string texturesAsset);
    void ReplaceModel(ObjectModel objectModel, string modelAsset, string texturesAsset);
    void ReplaceModelFor(Player player, Stream dff, Stream col, ObjectModel model);
    void RestoreModelFor(Player player, ObjectModel model);
}

internal sealed class AssetsService : IAssetsService
{
    public Action<ReplacedModel>? ModelReplaced { get; set; }
    public Action<Player, ObjectModel, Stream, Stream>? ReplaceModelForPlayer { get; set; }
    public Action<Player, ObjectModel>? RestoreModelForPlayer { get; set; }

    private readonly Dictionary<ObjectModel, ReplacedModel> _replacedModels = [];
    private readonly IAssetEncryptionProvider _assetEncryptionProvider;

    public IReadOnlyDictionary<ObjectModel, ReplacedModel> ReplacedModels => _replacedModels;

    public AssetsService(IAssetEncryptionProvider assetEncryptionProvider)
    {
        _assetEncryptionProvider = assetEncryptionProvider;
    }

    public LuaValue Map(IAsset asset)
    {
        return asset switch
        {
            FileSystemFont font => new LuaValue(new LuaValue[] { "FileSystemFont", font.Name, font.Path }),
            BuildInFont font => new LuaValue(new LuaValue[] { "MtaFont", font.Name }),
            AssetDFF dff => new LuaValue(new LuaValue[] { "DFF", dff.Name, _assetEncryptionProvider.TryEncryptPath(dff.Path) }),
            AssetCOL col => new LuaValue(new LuaValue[] { "COL", col.Name, _assetEncryptionProvider.TryEncryptPath(col.Path) }),
            AssetTXD txd => new LuaValue(new LuaValue[] { "TXD", txd.Name, _assetEncryptionProvider.TryEncryptPath(txd.Path) }),
            _ => throw new NotImplementedException()
        };
    }

    public void ReplaceModel(ObjectModel objectModel, string modelAsset, string collisionAsset, string texturesAsset)
    {
        if (_replacedModels.ContainsKey(objectModel))
            throw new InvalidOperationException("Model already replaced");

        var replacedModel = new ReplacedModel((ushort)objectModel, modelAsset, collisionAsset, texturesAsset);
        _replacedModels.Add(objectModel, replacedModel);
        ModelReplaced?.Invoke(replacedModel);
    }

    public void ReplaceModel(ObjectModel objectModel, string modelAsset, string texturesAsset)
    {
        if (_replacedModels.ContainsKey(objectModel))
            throw new InvalidOperationException("Model already replaced");

        var replacedModel = new ReplacedModel((ushort)objectModel, modelAsset, null, texturesAsset);
        _replacedModels.Add(objectModel, replacedModel);
        ModelReplaced?.Invoke(replacedModel);
    }

    public void ReplaceModelFor(Player player, Stream dff, Stream col, ObjectModel model)
    {
        ReplaceModelForPlayer?.Invoke(player, model, dff, col);
    }

    public void RestoreModelFor(Player player, ObjectModel model)
    {
        RestoreModelForPlayer?.Invoke(player, model);
    }
}

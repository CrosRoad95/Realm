using RealmCore.Resources.Assets.Classes;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;

namespace RealmCore.Resources.Assets;

public interface IAssetsService
{
    internal Action<ObjectModel, string, string>? ModelReplaced { get; set; }
    internal Action<Player, ObjectModel, Stream, Stream>? ReplaceModelForPlayer { get; set; }
    internal Action<Player, ObjectModel>? RestoreModelForPlayer { get; set; }

    LuaValue Map(IAsset asset);
    void ReplaceModel(ObjectModel objectModel, string dffName, string colName);
    void ReplaceModelFor(Player player, Stream dff, Stream col, ObjectModel model);
    void RestoreModelFor(Player player, ObjectModel model);
}

internal sealed class AssetsService : IAssetsService
{
    public Action<ObjectModel, string, string>? ModelReplaced { get; set; }
    public Action<Player, ObjectModel, Stream, Stream>? ReplaceModelForPlayer { get; set; }
    public Action<Player, ObjectModel>? RestoreModelForPlayer { get; set; }

    public LuaValue Map(IAsset asset)
    {
        return asset switch
        {
            FileSystemFont font => new LuaValue(new LuaValue[] { "FileSystemFont", font.Name, font.Path }),
            BuildInFont font => new LuaValue(new LuaValue[] { "MtaFont", font.Name }),
            AssetDFF dff => new LuaValue(new LuaValue[] { "DFF", dff.Name, dff.Path }),
            AssetCOL col => new LuaValue(new LuaValue[] { "COL", col.Name, col.Path }),
            _ => throw new NotImplementedException()
        };
    }

    public void ReplaceModel(ObjectModel objectModel, string dffName, string colName)
    {
        ModelReplaced?.Invoke(objectModel, dffName, colName);
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

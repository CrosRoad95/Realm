using RealmCore.Resources.Assets.Classes;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Assets;

public interface IAssetsService
{
    internal Action<Player, Stream, Stream, ushort>? ReplaceModelForPlayer { get; set; }
    internal Action<Player, ushort>? RestoreModelForPlayer { get; set; }

    LuaValue Map(IAsset asset);
    void ReplaceModelFor(Player player, Stream dff, Stream col, ushort model);
    void RestoreModelFor(Player player, ushort model);
}

internal sealed class AssetsService : IAssetsService
{
    public Action<Player, Stream, Stream, ushort>? ReplaceModelForPlayer { get; set; }
    public Action<Player, ushort>? RestoreModelForPlayer { get; set; }

    public LuaValue Map(IAsset asset)
    {
        return asset switch
        {
            FileSystemFont font => new LuaValue(new LuaValue[] { "FileSystemFont", font.Name, font.FontPath }),
            BuildInFont font => new LuaValue(new LuaValue[] { "MtaFont", font.Name }),
            Model model => new LuaValue(new LuaValue[] { "Model", model.Name, model.ColPath }),
            _ => throw new NotImplementedException()
        };
    }

    public void ReplaceModelFor(Player player, Stream dff, Stream col, ushort model)
    {
        ReplaceModelForPlayer?.Invoke(player, dff, col, model);
    }

    public void RestoreModelFor(Player player, ushort model)
    {
        RestoreModelForPlayer?.Invoke(player, model);
    }
}

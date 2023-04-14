using RealmCore.Resources.Assets.Classes;
using RealmCore.Resources.Assets.Interfaces;
using RenderWareIo.Structs.Col;
using RenderWareIo.Structs.Dff;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Assets;

internal sealed class AssetsService : IAssetsService
{
    public Action<Player, Stream, Stream, ushort>? ReplaceModelForPlayer { get; set; }
    public Action<Player, ushort>? RestoreModelForPlayer { get; set; }

    public LuaValue Map(IAsset asset)
    {
        return asset switch
        {
            Font font => new LuaValue(new LuaValue[] { "Font", font.FontPath }),
            Model model => new LuaValue(new LuaValue[] { "Model", model.ColPath }),
            _ => throw new NotImplementedException()
        };
    }

    public LuaValue MapHandle(IAsset asset)
    {
        return asset switch
        {
            Font font => new LuaValue(new LuaValue[] { "Font", font.Name }),
            Model model => new LuaValue(new LuaValue[] { "Model", model.Name }),
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

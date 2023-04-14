using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Assets.Interfaces;

public interface IAssetsService
{
    internal Action<Player, Stream, Stream, ushort>? ReplaceModelForPlayer { get; set; }
    internal Action<Player, ushort>? RestoreModelForPlayer { get; set; }

    LuaValue Map(IAsset asset);
    LuaValue MapHandle(IAsset asset);
    void ReplaceModelFor(Player player, Stream dff, Stream col, ushort model);
    void RestoreModelFor(Player player, ushort model);
}

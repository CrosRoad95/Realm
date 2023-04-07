using RealmCore.Resources.Assets.Interfaces;
using SlipeServer.Packets.Definitions.Lua;

namespace RealmCore.Resources.Assets;

public interface IAssetsService
{
    LuaValue Map(IAsset asset);
    LuaValue MapHandle(IAsset asset);
}

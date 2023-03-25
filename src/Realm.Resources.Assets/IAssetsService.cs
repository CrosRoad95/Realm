using Realm.Resources.Assets.Interfaces;
using SlipeServer.Packets.Definitions.Lua;

namespace Realm.Resources.Assets;

public interface IAssetsService
{
    LuaValue Map(IAsset asset);
    LuaValue MapHandle(IAsset asset);
}

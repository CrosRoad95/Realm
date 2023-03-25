using Realm.Resources.Assets.Classes;
using Realm.Resources.Assets.Interfaces;
using SlipeServer.Packets.Definitions.Lua;

namespace Realm.Resources.Assets;

internal sealed class AssetsService : IAssetsService
{
    public LuaValue Map(IAsset asset)
    {
        return asset switch
        {
            Font font => new LuaValue(new LuaValue[] { "Font", font.Path }),
            Model model => new LuaValue(new LuaValue[] { "Model", model.Path }),
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
}

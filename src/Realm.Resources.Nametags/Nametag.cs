using SlipeServer.Packets.Definitions.Lua;

namespace Realm.Resources.Nametags;

internal sealed class Nametag
{
    public string Text { get; set; }

    public LuaValue LuaValue => new(new Dictionary<LuaValue, LuaValue>
    {
        ["text"] = Text,
    });
}

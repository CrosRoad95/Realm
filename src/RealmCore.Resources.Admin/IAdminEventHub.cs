using SlipeServer.Packets.Definitions.Lua;

namespace RealmCore.Resources.Admin;

public interface IAdminEventHub
{
    void InternalAddOrUpdateDebugElements(IEnumerable<LuaValue> debugElements);
    void InternalSetAdminEnabled(bool enabled);
}

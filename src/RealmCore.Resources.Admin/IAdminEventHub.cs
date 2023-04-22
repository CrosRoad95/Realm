using SlipeServer.Packets.Definitions.Lua;

namespace RealmCore.Resources.Admin;

public interface IAdminEventHub
{
    void AddOrUpdateDebugElements(IEnumerable<LuaValue> debugElements);
    void SetAdminEnabled(bool enabled);
    void SetTools(IEnumerable<int> enabledTools);
}

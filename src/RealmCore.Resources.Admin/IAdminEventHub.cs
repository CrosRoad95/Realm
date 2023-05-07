using SlipeServer.Packets.Definitions.Lua;

namespace RealmCore.Resources.Admin;

public interface IAdminEventHub
{
    void SetAdminEnabled(bool enabled);
    void SetTools(IEnumerable<int> enabledTools);
    void AddOrUpdateEntity(IEnumerable<LuaValue> entityLuaValues);
    void ClearEntities();
    void SetSpawnMarkers(IEnumerable<LuaValue> spawnMarkers);
    void ClearSpawnMarkers();
    void UpdateEntitiesComponents(LuaValue entitiesComponents);
    void ClearEntitiesComponents();
}

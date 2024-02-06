namespace RealmCore.Resources.Admin;

public interface IAdminEventHub
{
    void SetAdminEnabled(bool enabled);
    void SetTools(IEnumerable<int> enabledTools);
    void AddOrUpdateElement(IEnumerable<LuaValue> elementsLuaValues);
    void ClearElements();
    void SetSpawnMarkers(IEnumerable<LuaValue> spawnMarkers);
    void ClearSpawnMarkers();
    void UpdateElementsComponents(LuaValue elementsComponents);
    void ClearElementsComponents();
}

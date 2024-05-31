namespace RealmCore.Resources.Overlay;

internal interface IHudEventHub
{
    void AddNotification(string message);
    void SetHudVisible(string hudId, bool visible);
    void SetHudPosition(string hudId, float px, float py);
    void SetHudState(string hudId, LuaValue state);
    void SetHud3dState(string hudId, LuaValue state);
    void CreateHud(string hudId, float px, float py, IEnumerable<LuaValue> hudElementsDefinitions);
    void CreateHud3d(string hudId, float px, float py, float pz, IEnumerable<LuaValue> hudElementsDefinitions);
    void RemoveHud(string hudId);
    void RemoveHud3d(string hudId);
    void AddDisplay3dRing(string id, float px, float py, float pz, double time);
    void RemoveDisplay3dRing(string id);
}

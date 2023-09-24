using RealmCore.Resources.Overlay.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Overlay;

public interface IOverlayService
{
    internal Action<Player, string>? NotificationAdded { get; set; }
    internal Action<Player, string, Vector2, IEnumerable<LuaValue>>? HudCreated { get; set; }
    internal Action<string, Vector3, IEnumerable<LuaValue>>? Hud3dCreated { get; set; }
    internal Action<Player, string>? HudRemoved { get; set; }
    internal Action<string>? Hud3dRemoved { get; set; }
    internal Action<Player, string, bool>? HudVisibilityChanged { get; set; }
    internal Action<Player, string, float, float>? HudPositionChanged { get; set; }
    internal Action<Player, string, Dictionary<int, object>>? HudStateChanged { get; set; }
    internal Action<string, Dictionary<int, object>>? Hud3dStateChanged { get; set; }
    internal Action<Player, string, Vector3, TimeSpan>? Display3dRingAdded { get; set; }
    internal Action<Player, string>? Display3dRingRemoved { get; set; }

    void AddNotification(Player player, string message);
    string AddRing3dDisplay(Player player, Vector3 position, TimeSpan time);
    void CreateHud(Player player, string hudId, Action<IHudBuilder<object>> hudBuilderCallback, Vector2 screenSize, Vector2? position = null);
    void CreateHud<TState>(Player player, string hudId, Action<IHudBuilder<TState>> hudBuilderCallback, Vector2 screenSize, Vector2? position = null, TState? defaultState = null) where TState : class;
    void CreateHud3d<TState>(string hudId, Action<IHudBuilder<TState>> hudBuilderCallback, TState state, Vector3? position = null) where TState : class;
    void RemoveHud(Player player, string hudId);
    void RemoveHud3d(string hudId);
    void RemoveRing3dDisplay(Player player, string id);
    void SetHud3dState(string hudId, Dictionary<int, object> state);
    void SetHudPosition(Player player, string hudId, Vector2 position);
    void SetHudState(Player player, string hudId, Dictionary<int, object> state);
    void SetHudVisible(Player player, string hudId, bool visible);
}

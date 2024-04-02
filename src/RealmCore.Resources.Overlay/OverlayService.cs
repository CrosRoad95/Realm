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
    internal Action<Player, string, Dictionary<int, object?>>? HudStateChanged { get; set; }
    internal Action<string, Dictionary<int, object?>>? Hud3dStateChanged { get; set; }
    internal Action<Player, string, Vector3, TimeSpan>? Display3dRingAdded { get; set; }
    internal Action<Player, string>? Display3dRingRemoved { get; set; }

    void AddNotification(Player player, string message);
    string AddRing3dDisplay(Player player, Vector3 position, TimeSpan time);
    void CreateHud(Player player, string hudId, Action<IHudBuilder<object>, IHudBuilderContext> hudBuilderCallback, Vector2 screenSize, Vector2? position = null);
    void CreateHud<TState>(Player player, string hudId, Action<IHudBuilder<TState>, IHudBuilderContext> hudBuilderCallback, Vector2 screenSize, Vector2? position, TState defaultState) where TState : class;
    void CreateHud3d<TState>(string hudId, Action<IHudBuilder<TState>, IHudBuilderContext> hudBuilderCallback, TState state, Vector3? position = null) where TState : class;
    void RemoveHudLayer(Player player, string hudId);
    void RemoveHud3d(string hudId);
    void RemoveRing3dDisplay(Player player, string id);
    void SetHud3dState(string hudId, Dictionary<int, object?> state);
    void SetHudPosition(Player player, string hudId, Vector2 position);
    void SetHudState(Player player, string hudId, Dictionary<int, object?> state);
    void SetHudVisible(Player player, string hudId, bool visible);
}

internal sealed class OverlayService : IOverlayService
{
    private struct HudElement
    {
        public string type;
        public string id;
        public Vector2 position;
        public Size size;

        public LuaValue LuaValue => new(new LuaValue[] { type, id, position.X, position.Y, size.Width, size.Height });
    }

    public Action<Player, string>? NotificationAdded { get; set; }
    public Action<Player, string, Vector2, IEnumerable<LuaValue>>? HudCreated { get; set; }
    public Action<string, Vector3, IEnumerable<LuaValue>>? Hud3dCreated { get; set; }
    public Action<Player, string>? HudRemoved { get; set; }
    public Action<string>? Hud3dRemoved { get; set; }
    public Action<Player, string, bool>? HudVisibilityChanged { get; set; }
    public Action<Player, string, float, float>? HudPositionChanged { get; set; }
    public Action<Player, string, Dictionary<int, object?>>? HudStateChanged { get; set; }
    public Action<string, Dictionary<int, object?>>? Hud3dStateChanged { get; set; }
    public Action<Player, string, Vector3, TimeSpan>? Display3dRingAdded { get; set; }
    public Action<Player, string>? Display3dRingRemoved { get; set; }
    private readonly IAssetsService _assetsService;

    public OverlayService(IAssetsService assetsService)
    {
        _assetsService = assetsService;
    }

    public void AddNotification(Player player, string message)
    {
        NotificationAdded?.Invoke(player, message);
    }

    public void SetHudVisible(Player player, string hudId, bool visible)
    {
        HudVisibilityChanged?.Invoke(player, hudId, visible);
    }

    public void SetHudPosition(Player player, string hudId, Vector2 position)
    {
        HudPositionChanged?.Invoke(player, hudId, position.X, position.Y);
    }

    public void SetHudState(Player player, string hudId, Dictionary<int, object?> state)
    {
        HudStateChanged?.Invoke(player, hudId, state);
    }

    public void SetHud3dState(string hudId, Dictionary<int, object?> state)
    {
        Hud3dStateChanged?.Invoke(hudId, state);
    }

    public void CreateHud3d<TState>(string hudId, Action<IHudBuilder<TState>, IHudBuilderContext> hudBuilderCallback, TState state, Vector3? position = null) where TState : class
    {
        var hudBuilder = new HudBuilder<TState>(state, _assetsService);
        var context = new HudBuilderContext(Vector2.Zero);
        hudBuilderCallback(hudBuilder, context);
        Hud3dCreated?.Invoke(hudId, position ?? Vector3.Zero, hudBuilder.HudElementsDefinitions);
    }

    public void CreateHud(Player player, string hudId, Action<IHudBuilder<object>, IHudBuilderContext> hudBuilderCallback, Vector2 screenSize, Vector2? position = null)
    {
        var hudBuilder = new HudBuilder<object>(new object(), _assetsService);
        var context = new HudBuilderContext(screenSize);
        hudBuilderCallback(hudBuilder, context);
        HudCreated?.Invoke(player, hudId, position ?? Vector2.Zero, hudBuilder.HudElementsDefinitions);
    }

    public void CreateHud<TState>(Player player, string hudId, Action<IHudBuilder<TState>, IHudBuilderContext> hudBuilderCallback, Vector2 screenSize, Vector2? position, TState defaultState) where TState : class
    {
        var hudBuilder = new HudBuilder<TState>(defaultState, _assetsService);
        var context = new HudBuilderContext(screenSize);
        hudBuilderCallback(hudBuilder, context);
        HudCreated?.Invoke(player, hudId, position ?? Vector2.Zero, hudBuilder.HudElementsDefinitions);
    }

    public void RemoveHudLayer(Player player, string hudId)
    {
        HudRemoved?.Invoke(player, hudId);
    }

    public void RemoveHud3d(string hudId)
    {
        Hud3dRemoved?.Invoke(hudId);
    }

    public string AddRing3dDisplay(Player player, Vector3 position, TimeSpan time)
    {
        var id = Guid.NewGuid().ToString();
        Display3dRingAdded?.Invoke(player, id, position, time);
        return id;
    }

    public void RemoveRing3dDisplay(Player player, string id)
    {
        Display3dRingRemoved?.Invoke(player, id);
    }
}

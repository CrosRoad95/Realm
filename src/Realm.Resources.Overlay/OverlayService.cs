using Realm.Resources.Assets;
using Realm.Resources.Overlay.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using System.Drawing;
using System.Numerics;

namespace Realm.Resources.Overlay;

public class OverlayService
{
    private struct HudElement
    {
        public string type;
        public string id;
        public Vector2 position;
        public Size size;

        public LuaValue LuaValue => new(new LuaValue[] { type, id, position.X, position.Y, size.Width, size.Height });
    }

    internal Action<Player, string>? NotificationAdded;
    internal Action<Player, string, Vector2, IEnumerable<LuaValue>>? HudCreated;
    internal Action<string, Vector3, IEnumerable<LuaValue>>? Hud3dCreated;
    internal Action<Player, string>? HudRemoved;
    internal Action<Player, string, bool>? HudVisibilityChanged;
    internal Action<Player, string, float, float>? HudPositionChanged;
    internal Action<Player, string, Dictionary<int, object>>? HudStateChanged;
    private readonly AssetsService _assetsService;

    public OverlayService(AssetsService assetsService)
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
    
    public void SetHudState(Player player, string hudId, Dictionary<int, object> state)
    {
        HudStateChanged?.Invoke(player, hudId, state);
    }

    public void CreateHud3d<TState>(string hudId, Action<IHudBuilder<TState>> hudBuilderCallback, Vector3? position = null) where TState : class
    {
        var hudBuilder = new HudBuilder<TState>(null, _assetsService, new Vector2(0,0));
        hudBuilderCallback(hudBuilder);
        Hud3dCreated?.Invoke(hudId, position ?? Vector3.Zero, hudBuilder.HudElementsDefinitions);
    }

    public void CreateHud(Player player, string hudId, Action<IHudBuilder<object>> hudBuilderCallback, Vector2 screenSize, Vector2 ? position = null)
    {
        var hudBuilder = new HudBuilder<object>(null, _assetsService, screenSize);
        hudBuilderCallback(hudBuilder);
        HudCreated?.Invoke(player, hudId, position ?? Vector2.Zero, hudBuilder.HudElementsDefinitions);
    }

    public void CreateHud<TState>(Player player, string hudId, Action<IHudBuilder<TState>> hudBuilderCallback, Vector2 screenSize, Vector2? position = null, TState? defaultState = null) where TState : class
    {
        var hudBuilder = new HudBuilder<TState>(defaultState, _assetsService, screenSize);
        hudBuilderCallback(hudBuilder);
        HudCreated?.Invoke(player, hudId, position ?? Vector2.Zero, hudBuilder.HudElementsDefinitions);
    }

    public void RemoveHud(Player player, string hudId)
    {
        HudRemoved?.Invoke(player, hudId);
    }
}

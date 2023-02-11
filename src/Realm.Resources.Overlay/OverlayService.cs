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
    internal Action<Player, string>? HudRemoved;
    internal Action<Player, string, bool>? HudVisibilityChanged;

    public void AddNotification(Player player, string message)
    {
        NotificationAdded?.Invoke(player, message);
    }

    public void SetHudVisible(Player player, string hudId, bool visible)
    {
        HudVisibilityChanged?.Invoke(player, hudId, visible);
    }
    
    public void CreateHud(Player player, string hudId, Action<IHudBuilder<object>> hudBuilderCallback, Vector2 screenSize, Vector2 ? position = null)
    {
        var hudBuilder = new HudBuilder<object>(null, screenSize);
        hudBuilderCallback(hudBuilder);
        HudCreated?.Invoke(player, hudId, position ?? Vector2.Zero, hudBuilder.HudElementsDefinitions);
    }

    public void CreateHud<TState>(Player player, string hudId, Action<IHudBuilder<TState>> hudBuilderCallback, Vector2 screenSize, Vector2? position = null, TState? defaultState = null) where TState : class
    {
        var hudBuilder = new HudBuilder<TState>(defaultState, screenSize);
        hudBuilderCallback(hudBuilder);
        HudCreated?.Invoke(player, hudId, position ?? Vector2.Zero, hudBuilder.HudElementsDefinitions);
    }

    public void RemoveHud(Player player, string hudId)
    {
        HudRemoved?.Invoke(player, hudId);
    }
}

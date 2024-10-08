﻿using SlipeServer.Server.Elements;
using System.ComponentModel;

namespace RealmCore.Resources.Overlay;

public enum ParticleEffect
{
    [Description("blood")]
    Blood,
    [Description("boat_splash")]
    BoatSplash,
    [Description("bubble")]
    Bubble,
    [Description("car_debris")]
    CarDebris,
    [Description("collision_smoke")]
    CollisionSmoke,
    [Description("gunshell")]
    Gunshell,
    [Description("sand")]
    Sand,
    [Description("sand2")]
    Sand2,
    [Description("huge_smoke")]
    HugeSmoke,
    [Description("smoke")]
    Smoke,
    [Description("spark")]
    Spark,
    [Description("spark2")]
    Spark2,
    [Description("splash")]
    Splash,
    [Description("wake")]
    Wake,
    [Description("water_splash")]
    WaterSplash,
    [Description("wheel_dirt")]
    WheelDirt,
    [Description("glass")]
    Glass,
}

public class Line3dEffect
{
    private readonly LuaValue _luaValue;

    public Line3dEffect(LuaValue luaValue)
    {
        _luaValue = luaValue;
    }

    public static Line3dEffect None = new Line3dEffect(new Dictionary<LuaValue, LuaValue>
    {
        ["type"] = "none"
    });

    public static Line3dEffect Gravity(float magnitude = 1, int segments = 12)
    {
        return new Line3dEffect(new Dictionary<LuaValue, LuaValue>
        {
            ["type"] = "gravity",
            ["magnitude"] = magnitude,
            ["segments"] = segments
        });
    }

    internal LuaValue AsLuaValue() => _luaValue;
}

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
    internal Action<Player, string, int, float, float, float, double, float, float, int, int>? BlipAdded { get; set; }
    internal Action<Player, string>? BlipRemoved { get; set; }
    internal Action<Player>? AllBlipsRemoved { get; set; }
    Action<string, float, float, float>? Hud3dPositionChanged { get; set; }
    Action<Player, string, int, Size>? ElementSizeChanged { get; set; }
    Action<Player, string, int, Vector2>? ElementPositionChanged { get; set; }
    Action<Player, string, int, bool>? ElementVisibleChanged { get; set; }
    Action<Player, string, int, LuaValue>? ElementContentChanged { get; set; }
    Action<IMessage>? MessageHandler { get; set; }

    void AddNotification(Player player, string message);
    string AddRing3dDisplay(Player player, Vector3 position, TimeSpan time);
    void CreateHud<TState>(Player player, string hudId, Action<IHudBuilder, IHudBuilderContext> hudBuilderCallback, Vector2 screenSize, Vector2? position, TState defaultState) where TState : class;
    void RemoveHudLayer(Player player, string hudId);
    void RemoveHud3d(string hudId);
    void RemoveRing3dDisplay(Player player, string id);
    void SetHud3dState(string hudId, Dictionary<int, object?> state);
    void SetHudPosition(Player player, string hudId, Vector2 position);
    void SetHudState(Player player, string hudId, Dictionary<int, object?> state);
    void SetHudVisible(Player player, string hudId, bool visible);
    void AddBlip(Player player, string id, int icon, Vector3 position, Color color, float visibleDistance, float size, int interior, int dimension);
    void RemoveBlip(Player player, string id);
    void RemoveAllBlips(Player player);
    void Set3dHudVisible(string hudId, bool visible);
    void SetHud3dPosition(string hudId, Vector3 position);
    void CreateHud3d<TState>(string hudId, Action<IHudBuilder, IHudBuilderContext> hudBuilderCallback, Vector3? position, TState? defaultState = null) where TState : class;
    void PositionChanged(Player player, string hudId, int elementId, Vector2 position);
    void SizeChanged(Player player, string hudId, int elementId, Size size);
    void VisibleChanged(Player player, string hudId, int elementId, bool visible);
    void SetHudContent(Player player, string hudId, int elementId, IHudElementContent hudElementContent, object? state);
    int CreateLine3d(IEnumerable<Player> players, PositionContext from, PositionContext to, Color color, float width, Line3dEffect effect);
    void RemoveLine3d(IEnumerable<Player> players, int[] lines);
    void AddEffect(IEnumerable<Player> players, ParticleEffect effect, PositionContext position, Vector3 direction, Color color, bool randomizeColors, int count = 1, float brightness = 1, float size = 0.3F, bool randomSizes = false, float life = 1);
}

internal sealed class OverlayService : IOverlayService
{
    private int _id;
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
    public Action<string, bool>? Hud3dVisibilityChanged { get; set; }
    public Action<Player, string, float, float>? HudPositionChanged { get; set; }
    public Action<string, float, float, float>? Hud3dPositionChanged { get; set; }
    public Action<Player, string, Dictionary<int, object?>>? HudStateChanged { get; set; }
    public Action<string, Dictionary<int, object?>>? Hud3dStateChanged { get; set; }
    public Action<Player, string, Vector3, TimeSpan>? Display3dRingAdded { get; set; }
    public Action<Player, string>? Display3dRingRemoved { get; set; }
    public Action<Player, string, int, float, float, float, double, float, float, int, int>? BlipAdded { get; set; }
    public Action<Player, string>? BlipRemoved { get; set; }
    public Action<Player>? AllBlipsRemoved { get; set; }
    public Action<Player, string, int, Vector2>? ElementPositionChanged { get; set; }
    public Action<Player, string, int, Size>? ElementSizeChanged { get; set; }
    public Action<Player, string, int, bool>? ElementVisibleChanged { get; set; }
    public Action<Player, string, int, LuaValue>? ElementContentChanged { get; set; }

    private readonly IAssetsService _assetsService;

    public Action<IMessage>? MessageHandler { get; set; }

    public OverlayService(IAssetsService assetsService)
    {
        _assetsService = assetsService;
    }

    public int CreateLine3d(IEnumerable<Player> players, PositionContext from, PositionContext to, Color color, float width, Line3dEffect effect)
    {
        var id = Interlocked.Increment(ref _id);
        MessageHandler?.Invoke(new CreateLine3dMessage(players, id, from, to, color, width, effect.AsLuaValue()));
        return id;
    }

    public void RemoveLine3d(IEnumerable<Player> players, int[] lines)
    {
        MessageHandler?.Invoke(new RemoveLine3dMessage(players, lines));
    }

    public void AddEffect(IEnumerable<Player> players, ParticleEffect effect, PositionContext position, Vector3 direction, Color color, bool randomizeColors, int count = 1, float brightness = 1.0f, float size = 0.3f, bool randomSizes = false, float life = 1.0f)
    {
        MessageHandler?.Invoke(new AddEffect3dMessage(players, effect, position, direction, color, randomizeColors, count, brightness, size, randomSizes, life));
    }

    public void AddNotification(Player player, string message)
    {
        NotificationAdded?.Invoke(player, message);
    }

    public void SetHudVisible(Player player, string hudId, bool visible)
    {
        HudVisibilityChanged?.Invoke(player, hudId, visible);
    }

    public void Set3dHudVisible(string hudId, bool visible)
    {
        Hud3dVisibilityChanged?.Invoke(hudId, visible);
    }

    public void SetHudPosition(Player player, string hudId, Vector2 position)
    {
        HudPositionChanged?.Invoke(player, hudId, position.X, position.Y);
    }
    
    public void SetHud3dPosition(string hudId, Vector3 position)
    {
        Hud3dPositionChanged?.Invoke(hudId, position.X, position.Y, position.Z);
    }

    public void SetHudState(Player player, string hudId, Dictionary<int, object?> state)
    {
        HudStateChanged?.Invoke(player, hudId, state);
    }

    public void SetHud3dState(string hudId, Dictionary<int, object?> state)
    {
        Hud3dStateChanged?.Invoke(hudId, state);
    }

    public void CreateHud3d<TState>(string hudId, Action<IHudBuilder, IHudBuilderContext> hudBuilderCallback, Vector3? position, TState? defaultState = null) where TState : class
    {
        var hudBuilder = new HudBuilder(_assetsService, defaultState);
        var context = new HudBuilderContext(Vector2.Zero);
        hudBuilderCallback(hudBuilder, context);
        Hud3dCreated?.Invoke(hudId, position ?? Vector3.Zero, hudBuilder.HudElementsDefinitions);
    }

    public void CreateHud<TState>(Player player, string hudId, Action<IHudBuilder, IHudBuilderContext> hudBuilderCallback, Vector2 screenSize, Vector2? position, TState defaultState) where TState : class
    {
        var hudBuilder = new HudBuilder(_assetsService, defaultState);
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

    public void AddBlip(Player player, string id, int icon, Vector3 position, Color color, float visibleDistance, float size, int interior, int dimension)
    {
        BlipAdded?.Invoke(player, id, icon, position.X, position.Y, position.Z, color.ToLuaColor(), visibleDistance, size, interior, dimension);
    }

    public void RemoveBlip(Player player, string id)
    {
        BlipRemoved?.Invoke(player, id);
    }

    public void RemoveAllBlips(Player player)
    {
        AllBlipsRemoved?.Invoke(player);
    }

    public void PositionChanged(Player player, string hudId, int elementId, Vector2 position)
    {
        ElementPositionChanged?.Invoke(player, hudId, elementId, position);
    }

    public void SizeChanged(Player player, string hudId, int elementId, Size size)
    {
        ElementSizeChanged?.Invoke(player, hudId, elementId, size);
    }

    public void VisibleChanged(Player player, string hudId, int elementId, bool visible)
    {
        ElementVisibleChanged?.Invoke(player, hudId, elementId, visible);
    }

    public void SetHudContent(Player player, string hudId, int elementId, IHudElementContent hudElementContent, object? state)
    {
        ElementContentChanged?.Invoke(player, hudId, elementId, hudElementContent.CreateLuaValue(state));
    }
}

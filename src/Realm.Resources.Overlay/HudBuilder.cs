using Realm.Resources.Assets;
using Realm.Resources.Assets.Interfaces;
using Realm.Resources.Overlay.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using System.Drawing;
using System.Numerics;

namespace Realm.Resources.Overlay;

internal class HudBuilder<TState> : IHudBuilder<TState>
{
    private readonly List<LuaValue> _luaValues = new();
    private readonly TState? _defaultState;
    private readonly AssetsService _assetsService;
    private readonly Vector2 _screenSize;
    public float Right => _screenSize.X;
    public float Bottom => _screenSize.Y;
    private int id = 0;

    internal IEnumerable<LuaValue> HudElementsDefinitions => _luaValues;
    public HudBuilder(TState? defaultState, AssetsService assetsService, Vector2 screenSize)
    {
        _defaultState = defaultState;
        _assetsService = assetsService;
        _screenSize = screenSize;
    }

    public IHudBuilder<TState> InternalAddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, LuaValue? font = null, string alignX = "left", string alignY = "top")
    {
        color ??= Color.White;
        double luaColor = color.Value.B + color.Value.G * 256 + color.Value.R * 256 * 256 + color.Value.A * 256 * 256 * 256;
        _luaValues.Add(new(new LuaValue[] { "text", ++id, text, position.X, position.Y, size.Width, size.Height, luaColor, scale?.Width ?? 1, scale?.Height ?? 1, font, alignX, alignY }));
        return this;
    }
    
    public IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", string alignX = "left", string alignY = "top")
    {
        InternalAddText(text, position, size, color, scale, font, alignX, alignY);
        return this;
    }
    
    public IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, IFont? font = null, string alignX = "left", string alignY = "top")
    {
        if (font == null)
            throw new ArgumentNullException(nameof(font));
        InternalAddText(text, position, size, color, scale, _assetsService.MapHandle(font), alignX, alignY);
        return this;
    }

    public IHudBuilder<TState> AddRectangle(Vector2 position, Size size, Color color)
    {
        double luaColor = color.B + color.G * 256 + color.R * 256 * 256 + color.A * 256 * 256 * 256;
        _luaValues.Add(new(new LuaValue[] { "rectangle", ++id, position.X, position.Y, size.Width, size.Height, luaColor }));
        return this;
    }
}

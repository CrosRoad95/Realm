using RealmCore.Resources.Assets.Interfaces;
using RealmCore.Resources.Base.Extensions;
using RealmCore.Resources.Overlay.Builders;
using RealmCore.Resources.Overlay.Builders.Interfaces;
using RealmCore.Resources.Overlay.Enums;
using RealmCore.Resources.Overlay.Extensions;
using RealmCore.Resources.Overlay.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using System.Drawing;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace RealmCore.Resources.Overlay;

internal class HudBuilder<TState> : IHudBuilder<TState>
{
    private readonly List<LuaValue> _luaValues = new();
    private readonly TState? _state;
    private readonly IAssetsService _assetsService;
    private readonly Vector2 _screenSize;

    public float Right => _screenSize.X;
    public float Bottom => _screenSize.Y;
    private int _id = 0;

    internal IEnumerable<LuaValue> HudElementsDefinitions => _luaValues;
    public event Action<int, PropertyInfo>? DynamicHudComponentAdded;

    public HudBuilder(TState? defaultState, IAssetsService assetsService, Vector2 screenSize)
    {
        _state = defaultState;
        _assetsService = assetsService;
        _screenSize = screenSize;
    }

    internal IHudBuilder<TState> InternalAddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, LuaValue? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        color ??= Color.White;
        double luaColor = color.Value.ToLuaColor();
        _luaValues.Add(new(new LuaValue[] { "text", ++_id, text, position.X, position.Y, size.Width, size.Height, luaColor, scale?.Width ?? 1, scale?.Height ?? 1, font, alignX.AsString(), alignY.AsString() }));
        return this;
    }

    public IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        InternalAddText(text, position, size, color, scale, font, alignX, alignY);
        return this;
    }

    public IHudBuilder<TState> AddText(Action<ITextHudBuilder<TState>> textBuilderCallback)
    {
        var builder = new TextHudBuilder<TState>(++_id, _state, _assetsService);
        textBuilderCallback(builder);
        _luaValues.Add(builder.Build());
        return this;
    }

    public IHudBuilder<TState> AddText(Expression<Func<TState, string>> exp, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        var me = exp.Body as MemberExpression;
        var propertyInfo = me.Member as PropertyInfo;
        InternalAddText((string)propertyInfo.GetValue(_state), position, size, color, scale, font, alignX, alignY);
        DynamicHudComponentAdded?.Invoke(_id, propertyInfo);
        return this;
    }

    public IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, IFont? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        if (font == null)
            throw new ArgumentNullException(nameof(font));
        InternalAddText(text, position, size, color, scale, _assetsService.MapHandle(font), alignX, alignY);
        return this;
    }

    public IHudBuilder<TState> AddRectangle(Vector2 position, Size size, Color color)
    {
        double luaColor = color.ToLuaColor();
        _luaValues.Add(new(new LuaValue[] { "rectangle", ++_id, position.X, position.Y, size.Width, size.Height, luaColor }));
        return this;
    }
}

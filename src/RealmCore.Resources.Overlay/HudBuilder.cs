using RealmCore.Resources.Assets.Interfaces;
using RealmCore.Resources.Base.Extensions;
using RealmCore.Resources.Overlay.Builders;
using RealmCore.Resources.Overlay.Builders.Interfaces;
using RealmCore.Resources.Overlay.Enums;
using RealmCore.Resources.Overlay.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using System.Drawing;
using System.Linq.Expressions;
using System.Numerics;

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
    public Action<DynamicHudComponent>? DynamicHudComponentAdded { get; set; }

    public HudBuilder(TState? defaultState, IAssetsService assetsService, Vector2 screenSize)
    {
        _state = defaultState;
        _assetsService = assetsService;
        _screenSize = screenSize;
    }

    public IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        AddText(b => b.WithText(text).WithPosition(position).WithSize(size).WithColor(color ?? Color.White).WithFont(font).WithHorizontalAlign(alignX).WithVerticalAlign(alignY));
        return this;
    }

    public IHudBuilder<TState> AddText(Action<ITextHudBuilder<TState>> textBuilderCallback)
    {
        var builder = new TextHudBuilder<TState>(++_id, _state, _assetsService)
        {
            DynamicHudComponentAdded = DynamicHudComponentAdded
        };

        try
        {
            textBuilderCallback(builder);
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            builder.DynamicHudComponentAdded = null;
        }
        _luaValues.Add(builder.Build());
        return this;
    }

    public IHudBuilder<TState> AddText(Expression<Func<TState, string>> exp, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        AddText(b => b.WithText(exp).WithPosition(position).WithSize(size).WithScale(scale).WithColor(color ?? Color.White).WithFont(font).WithHorizontalAlign(alignX).WithVerticalAlign(alignY));
        return this;
    }

    public IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, IFont? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        if (font == null)
            throw new ArgumentNullException(nameof(font));

        AddText(b => b.WithText(text).WithPosition(position).WithSize(size).WithScale(scale).WithColor(color ?? Color.White).WithFont(font).WithHorizontalAlign(alignX).WithVerticalAlign(alignY));
        return this;
    }

    public IHudBuilder<TState> AddRectangle(Vector2 position, Size size, Color color)
    {
        double luaColor = color.ToLuaColor();
        _luaValues.Add(new(new LuaValue[] { "rectangle", ++_id, position.X, position.Y, size.Width, size.Height, luaColor }));
        return this;
    }
}

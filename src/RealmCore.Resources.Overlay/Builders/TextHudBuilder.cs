using RealmCore.Resources.Assets.Interfaces;
using RealmCore.Resources.Overlay.Enums;
using SlipeServer.Packets.Definitions.Lua;
using System.Drawing;
using System.Numerics;
using RealmCore.Resources.Base.Extensions;
using RealmCore.Resources.Overlay.Builders.Interfaces;
using RealmCore.Resources.Overlay.Extensions;
using System.Reflection;
using System.Linq.Expressions;

namespace RealmCore.Resources.Overlay.Builders;

internal class PropertyExpressionVisitor : ExpressionVisitor
{
    public List<PropertyInfo> Properties { get; private set; } = new();

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Member is PropertyInfo propertyInfo)
        {
            Properties.Add(propertyInfo);
        }
        return base.VisitMember(node);
    }
}

internal class TextHudBuilder<TState> : ITextHudBuilder<TState>
{
    private string _text = "";
    private Vector2 _position;
    private Size _size;
    private Color _color = Color.White;
    private Size? _scale;
    private string _stringFont = "default";
    private IFont? _font;
    private HorizontalAlign _alignX;
    private VerticalAlign _alignY;
    private readonly int _id;
    private readonly TState _state;
    private readonly IAssetsService _assetsService;
    public Action<DynamicHudComponent>? DynamicHudComponentAdded { get; set; }

    public TextHudBuilder(int id, TState state, IAssetsService assetsService)
    {
        _id = id;
        _state = state;
        _assetsService = assetsService;
    }

    public ITextHudBuilder<TState> WithText(string text)
    {
        _text = text;
        return this;
    }

    private TProperty WithProperty<TProperty>(Expression<Func<TState, TProperty>> expression)
    {
        var visitor = new PropertyExpressionVisitor();
        visitor.Visit(expression);
        var factory = expression.Compile();
        var value = factory(_state);
        foreach (var propertyInfo in visitor.Properties)
            DynamicHudComponentAdded?.Invoke(new DynamicHudComponent
            {
                ComponentId = _id,
                PropertyInfo = propertyInfo,
                Factory = factory
            });

        return value;
    }

    public ITextHudBuilder<TState> WithText(Expression<Func<TState, string>> expression)
    {
        _text = WithProperty(expression);
        return this;
    }

    public ITextHudBuilder<TState> WithPosition(Expression<Func<TState, Vector2>> expression)
    {
        _position = WithProperty(expression);
        return this;
    }
    
    public ITextHudBuilder<TState> WithPosition(Vector2 position)
    {
        _position = position;
        return this;
    }

    public ITextHudBuilder<TState> WithSize(Size size)
    {
        _size = size;
        return this;
    }

    public ITextHudBuilder<TState> WithColor(Color color)
    {
        _color = color;
        return this;
    }

    public ITextHudBuilder<TState> WithScale(Size? scale)
    {
        _scale = scale;
        return this;
    }

    public ITextHudBuilder<TState> WithFont(IFont font)
    {
        _font = font;
        return this;
    }

    public ITextHudBuilder<TState> WithFont(string stringFont)
    {
        _stringFont = stringFont;
        return this;
    }

    public ITextHudBuilder<TState> WithHorizontalAlign(HorizontalAlign alignX)
    {
        _alignX = alignX;
        return this;
    }

    public ITextHudBuilder<TState> WithVerticalAlign(VerticalAlign alignY)
    {
        _alignY = alignY;
        return this;
    }

    public LuaValue[] Build()
    {
        return new LuaValue[]
        {
            "text",
            _id,
            _text,
            _position.X,
            _position.Y,
            _size.Width,
            _size.Height,
            _color.ToLuaColor(),
            _scale?.Width ?? 1,
            _scale?.Height ?? 1,
            (_font != null) ? _assetsService.MapHandle(_font) : _stringFont,
            _alignX.AsString(),
            _alignY.AsString()
        };
    }
}
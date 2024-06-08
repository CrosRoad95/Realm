using System.Reflection;
using Color = System.Drawing.Color;

namespace RealmCore.Resources.Overlay;

public enum AddElementLocation
{
    Default,
    AtTheBeginning,
    BeforeLastElement
}

public interface IHudElement
{
    internal IHudElementContent? Content { get; }
    internal LuaValue CreateLuaValue(object state, int id, IAssetsService assetsService);
}

public struct RectangleHudElement : IHudElement
{
    private readonly Vector2 _position;
    private readonly Size _size;
    private readonly Color _color;

    public IHudElementContent? Content { get; } = null;

    public RectangleHudElement(Vector2 position, Size size, Color color)
    {
        _position = position;
        _size = size;
        _color = color;
    }

    public LuaValue CreateLuaValue(object state, int id, IAssetsService assetsService)
    {
        return new LuaValue(new LuaValue[] { "rectangle", id, _position.X, _position.Y, _size.Width, _size.Height, _color.ToLuaColor() });
    }
}

public interface IHudElementContent;

public interface ITextHudElementContent : IHudElementContent
{
    internal LuaValue CreateLuaValue(object state);
}

public interface IComputedTextHudElementContent : ITextHudElementContent
{
    internal Delegate Factory { get; }
}

public struct ConstantTextHudElementContent : ITextHudElementContent
{
    public string Content { get; }

    public ConstantTextHudElementContent(string content)
    {
        Content = content;
    }

    public LuaValue CreateLuaValue(object state) => new LuaValue(["constant", Content]);
}

public struct CurrentVehicleSpeedTextHudElementContent : ITextHudElementContent
{
    public LuaValue CreateLuaValue(object state) => new LuaValue(["computed", "vehicleSpeed"]);
}

internal class PropertyExpressionVisitor : ExpressionVisitor
{
    public List<PropertyInfo> Properties { get; private set; } = [];

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Member is PropertyInfo propertyInfo)
        {
            Properties.Add(propertyInfo);
        }
        return base.VisitMember(node);
    }
}

public struct StatePropertyTextHudElementContent : IComputedTextHudElementContent
{
    public Delegate Factory { get; }

    public StatePropertyTextHudElementContent(Expression expression)
    {
        if (expression is LambdaExpression lambdaExpression)
        {
            Factory = lambdaExpression.Compile();
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public static StatePropertyTextHudElementContent Create<TState, TProperty>(Expression<Func<TState, TProperty>> expression)
    {
        return new StatePropertyTextHudElementContent(expression);
    }

    public LuaValue CreateLuaValue(object state){

        return new LuaValue(["constant", Factory.DynamicInvoke(state)?.ToString() ?? ""]);
    }
}

public struct TextHudElement : IHudElement
{
    private readonly ITextHudElementContent _content;
    private readonly Vector2 _position;
    private readonly Size _size;
    private readonly Color _color;
    private readonly Size _scale;
    private readonly IFont _font;
    private readonly HorizontalAlign _alignX;
    private readonly VerticalAlign _alignY;
    public IHudElementContent? Content => _content;

    public TextHudElement(ITextHudElementContent text, Vector2 position, Size size, Color? color = null, Size? scale = null, IFont? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        _content = text;
        _position = position;
        _size = size;
        _color = color ?? Color.White;
        _scale = scale ?? new Size(1,1);
        _font = font ?? BuildInFonts.Default;
        _alignX = alignX;
        _alignY = alignY;
    }

    public TextHudElement(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, IFont? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top) : this(new ConstantTextHudElementContent(text), position, size, color, scale, font, alignX, alignY) { }

    public LuaValue CreateLuaValue(object state, int id, IAssetsService assetsService)
    {
        var font = assetsService.Map(_font);
        LuaValue content = _content.CreateLuaValue(state);
        return new LuaValue(
        new LuaValue[] { "text", id, content, _position.X, _position.Y, _size.Width, _size.Height, _color.ToLuaColor(), _scale.Width, _scale.Height, font, _alignX.AsString(), _alignY.AsString() });
    }
}

internal sealed class HudBuilder : IHudBuilder
{
    private readonly List<LuaValue> _luaValues = [];
    private readonly object _state;
    private readonly IAssetsService _assetsService;
    private int _id = 0;

    internal IEnumerable<LuaValue> HudElementsDefinitions => _luaValues;
    public Action<DynamicHudElement>? DynamicHudElementAdded { get; set; }

    public HudBuilder(object defaultState, IAssetsService assetsService)
    {
        _state = defaultState;
        _assetsService = assetsService;
    }

    private void AddLuaValue(LuaValue luaValue, AddElementLocation addElementLocation = AddElementLocation.Default)
    {
        switch (addElementLocation)
        {
            case AddElementLocation.Default:
                _luaValues.Add(luaValue);
                break;
            case AddElementLocation.AtTheBeginning:
                _luaValues.Insert(0, luaValue);
                break;
            case AddElementLocation.BeforeLastElement:
                _luaValues.Insert(_luaValues.Count - 1, luaValue);
                break;
            default:
                break;
        }
    }

    public int AllocateId() => Interlocked.Increment(ref _id);

    public IHudBuilder Add(IHudElement hudElement, AddElementLocation addElementLocation = AddElementLocation.Default)
    {
        var id = AllocateId();
        var luaValue = hudElement.CreateLuaValue(_state, id, _assetsService);
        AddLuaValue(luaValue, addElementLocation);

        if (hudElement.Content is IComputedTextHudElementContent content)
        {
            DynamicHudElementAdded?.Invoke(new DynamicHudElement
            {
                Id = id,
                Factory = content.Factory
            });
        }

        return this;
    }
}

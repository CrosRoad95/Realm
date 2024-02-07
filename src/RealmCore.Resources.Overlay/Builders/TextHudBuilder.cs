using RealmCore.Resources.Assets.Interfaces;
using RealmCore.Resources.Overlay.Enums;
using RealmCore.Resources.Overlay.Builders.Interfaces;
using System.Reflection;
using System.Linq.Expressions;
using RealmCore.Resources.Overlay.ConstructionInfos;

namespace RealmCore.Resources.Overlay.Builders;

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

internal class TextHudBuilder<TState> : ITextHudBuilder<TState>
{
    private ComputedValueType? _computedValueType = null;
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
    public bool IsDynamic { get; private set; }
    private List<PropertyInfo> _propertyInfoList = [];

    public Action<DynamicHudElement>? DynamicHudElementAdded { get; set; }

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
    
    public ITextHudBuilder<TState> WithComputedValue(ComputedValueType computedValueType)
    {
        _computedValueType = computedValueType;
        return this;
    }

    private TProperty WithProperty<TProperty>(Expression<Func<TState, TProperty>> expression)
    {
        var visitor = new PropertyExpressionVisitor();
        visitor.Visit(expression);
        var factory = expression.Compile();
        var value = factory(_state);
        _propertyInfoList = visitor.Properties;
        foreach (var propertyInfo in visitor.Properties)
            DynamicHudElementAdded?.Invoke(new DynamicHudElement
            {
                Id = _id,
                PropertyInfo = propertyInfo,
            });

        IsDynamic = true;
        return value;
    }

    public ITextHudBuilder<TState> WithText(Expression<Func<TState, string>> expression)
    {
        _text = WithProperty(expression) ?? string.Empty;
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

    public TextConstructionInfo Build()
    {
        if (_computedValueType == null)
        {
            return new TextConstructionInfo
            {
                isDynamic = IsDynamic,
                propertyInfos = _propertyInfoList,
                parentId = _id,
                id = _id,
                text = _text,
                position = _position,
                size = _size,
                color = _color,
                scale = _scale ?? new Size(1,1),
                font = (_font != null) ? _assetsService.MapHandle(_font) : _stringFont,
                alignX = _alignX,
                alignY = _alignY
            };
        }

        return new TextConstructionInfo
        {
            isComputed = true,
            parentId = _id,
            id = _id,
            text = _computedValueType.Value.ToString(),
            position = _position,
            size = _size,
            color = _color,
            scale = _scale ?? new Size(1, 1),
            font = (_font != null) ? _assetsService.MapHandle(_font) : _stringFont,
            alignX = _alignX,
            alignY = _alignY
        };
    }
}
namespace RealmCore.Resources.Overlay;

internal static class HudElementType
{
    public const string Rectangle = "rectangle";
    public const string Text = "text";
    public const string Radar = "radar";
}

internal static class HudElementContentType
{
    public const string Constant = "constant";
    public const string Computed = "computed";
    public const string RemoteImage = "remoteImage";
    public const string Image = "image";
}

internal static class HudElementContentComputedType
{
    public const string VehicleSpeed = "vehicleSpeed";
    public const string FPS = "fps";
}

public enum AddElementLocation
{
    Default,
    AtTheBeginning,
    BeforeLastElement
}

public enum PositioningMode
{
    Relative,
    Absolute
}

public interface IHudElement
{
    internal IHudElementContent? Content { get; }
    internal LuaValue CreateLuaValue(object? state, int id, IAssetsService assetsService);
}

public interface IHudElementContent
{
    internal LuaValue CreateLuaValue(object? state);
}

public interface ITextHudElementContent : IHudElementContent;

public interface IImageHudElementContent : IHudElementContent
{
    IImageAsset ImageAsset { get; }
}

public interface IComputedTextHudElementContent : ITextHudElementContent
{
    internal Delegate Factory { get; }
}

public readonly struct RectangleHudElement : IHudElement
{
    private readonly Vector2 _position;
    private readonly Size _size;
    private readonly Color _color;
    private readonly PositioningMode _positioningMode;

    public IHudElementContent? Content { get; } = null;

    public RectangleHudElement(Vector2 position, Size size, Color color, PositioningMode positioningMode = PositioningMode.Relative)
    {
        _position = position;
        _size = size;
        _color = color;
        _positioningMode = positioningMode;
    }

    public LuaValue CreateLuaValue(object? state, int id, IAssetsService assetsService)
    {
        return new LuaTable
        {
            ["type"] = HudElementType.Rectangle,
            ["id"] = id,
            ["position"] = LuaValue.ArrayFromVector(_position),
            ["positioningMode"] = _positioningMode.ToString(),
            ["size"] = _size.ToLuaArray(),
            ["color"] = _color.ToLuaColor(),
        };
    }
}

public readonly struct TextHudElement : IHudElement
{
    private readonly ITextHudElementContent _content;
    private readonly Vector2 _position;
    private readonly Size _size;
    private readonly Color _color;
    private readonly Size _scale;
    private readonly IFont _font;
    private readonly HorizontalAlign _alignX;
    private readonly VerticalAlign _alignY;
    private readonly PositioningMode _positioningMode;

    public IHudElementContent? Content => _content;

    public TextHudElement(ITextHudElementContent text, Vector2 position, Size size, Color? color = null, Size? scale = null, IFont? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top, PositioningMode positioningMode = PositioningMode.Relative)
    {
        _content = text;
        _position = position;
        _size = size;
        _color = color ?? Color.White;
        _scale = scale ?? new Size(1, 1);
        _font = font ?? BuildInFonts.Default;
        _alignX = alignX;
        _alignY = alignY;
        _positioningMode = positioningMode;
    }

    public TextHudElement(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, IFont? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top, PositioningMode positioningMode = PositioningMode.Relative) : this(new ConstantTextHudElementContent(text), position, size, color, scale, font, alignX, alignY, positioningMode) { }

    public LuaValue CreateLuaValue(object? state, int id, IAssetsService assetsService)
    {
        var font = assetsService.Map(_font);
        LuaValue content = _content.CreateLuaValue(state);
        return new LuaTable
        {
            ["type"] = HudElementType.Text,
            ["id"] = id,
            ["position"] = LuaValue.ArrayFromVector(_position),
            ["positioningMode"] = _positioningMode.ToString(),
            ["content"] = content,
            ["size"] = _size.ToLuaArray(),
            ["scale"] = _scale.ToLuaArray(),
            ["color"] = _color.ToLuaColor(),
            ["font"] = font,
            ["align"] = new LuaValue[] { _alignX.AsString(), _alignY.AsString() },
        };
    }
}

public readonly struct RadarHudElement : IHudElement
{
    private readonly Vector2 _position;
    private readonly Size _size;
    private readonly PositioningMode _positioningMode;
    private readonly IImageHudElementContent _content;
    private readonly Dictionary<int, IImageAsset> _blips;

    public IHudElementContent? Content => _content;

    public RadarHudElement(Vector2 position, Size size, IImageHudElementContent content, Dictionary<int, IImageAsset> blips, PositioningMode positioningMode = PositioningMode.Relative)
    {
        _position = position;
        _size = size;
        _content = content;
        _blips = blips;
        _positioningMode = positioningMode;
    }

    public LuaValue CreateLuaValue(object? state, int id, IAssetsService assetsService)
    {
        var image = assetsService.Map(_content.ImageAsset);

        return new LuaTable
        {
            ["type"] = HudElementType.Radar,
            ["id"] = id,
            ["position"] = LuaValue.ArrayFromVector(_position),
            ["positioningMode"] = _positioningMode.ToString(),
            ["size"] = _size.ToLuaArray(),
            ["image"] = image,
            ["blips"] = _blips.ToDictionary(x => (LuaValue)x.Key, x => assetsService.Map(x.Value)),
        };
    }
}

public readonly struct ConstantTextHudElementContent : ITextHudElementContent
{
    public string Content { get; }

    public ConstantTextHudElementContent(string content)
    {
        Content = content;
    }

    public LuaValue CreateLuaValue(object? state) => new LuaTable
    {
        ["type"] = HudElementContentType.Constant,
        ["value"] = Content
    };
}

public readonly struct ImageHudElementContent : IImageHudElementContent
{
    private readonly IImageAsset _imageAsset;

    public string Path => _imageAsset.Path;
    public IImageAsset ImageAsset => _imageAsset;

    public ImageHudElementContent(IImageAsset imageAsset)
    {
        _imageAsset = imageAsset;
    }

    public LuaValue CreateLuaValue(object? state)
    {
        if (_imageAsset is IRemoteImageAsset)
        {
            return new LuaTable{
                ["type"] = HudElementContentType.RemoteImage,
                ["path"] = Path
            };
        }
        else
        {
            return new LuaTable
            {
                ["type"] = HudElementContentType.Image,
                ["path"] = Path
            };
        }
    }
}

public readonly struct CurrentVehicleSpeedTextHudElementContent : ITextHudElementContent
{
    public LuaValue CreateLuaValue(object? state) => new LuaTable
    {
        ["type"] = HudElementContentType.Computed,
        ["value"] = HudElementContentComputedType.VehicleSpeed
    };
}

public readonly struct CurrentFPSTextHudElementContent : ITextHudElementContent
{
    public LuaValue CreateLuaValue(object? state) => new LuaTable
    {
        ["type"] = HudElementContentType.Computed,
        ["value"] = HudElementContentComputedType.FPS
    };
}

public readonly struct StatePropertyTextHudElementContent : IComputedTextHudElementContent
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

    public LuaValue CreateLuaValue(object? state)
    {
        var value = Factory.DynamicInvoke(state)?.ToString() ?? "";
        return new LuaTable
        {
            ["type"] = HudElementContentType.Constant,
            ["value"] = value
        };
    }
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

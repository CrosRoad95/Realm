﻿namespace RealmCore.Resources.Overlay;

internal static class HudElementType
{
    public const string Rectangle = "rectangle";
    public const string Text = "text";
    public const string Radar = "radar";
    public const string Image = "image";
}

internal static class HudElementContentType
{
    public const string Constant = "constant";
    public const string ConstantNumber = "constantNumber";
    public const string Computed = "computed";
    public const string RemoteImage = "remoteImage";
    public const string Image = "image";
}

internal static class HudElementContentComputedType
{
    public const string VehicleSpeedText = "vehicleSpeedText";
    public const string VehicleSpeedNummber = "vehicleSpeedNummber";
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

public readonly struct ImageHudElement : IHudElement
{
    private readonly Vector2 _position;
    private readonly Size _size;
    private readonly ITextHudElementContent _rotation;
    private readonly Vector2 _rotationCenterOffset;
    private readonly PositioningMode _positioningMode;

    public IHudElementContent Content { get; init; }

    public ImageHudElement(Vector2 position, Size size, IImageHudElementContent content, ITextHudElementContent? rotation = null, Vector2 rotationCenterOffset = default, PositioningMode positioningMode = PositioningMode.Relative)
    {
        _position = position;
        _size = size;
        Content = content;
        _rotation = rotation ?? new ConstantNumberHudElementContent(0);
        _rotationCenterOffset = rotationCenterOffset;
        _positioningMode = positioningMode;
    }

    public LuaValue CreateLuaValue(object? state, int id, IAssetsService assetsService)
    {
        var content = Content.CreateLuaValue(state);
        return new LuaTable
        {
            ["type"] = HudElementType.Image,
            ["id"] = id,
            ["position"] = LuaValue.ArrayFromVector(_position),
            ["positioningMode"] = _positioningMode.ToString(),
            ["size"] = _size.ToLuaArray(),
            ["rotation"] = _rotation.CreateLuaValue(null),
            ["rotationOffset"] = LuaValue.ArrayFromVector(_rotationCenterOffset),
            ["content"] = content
        };
    }
}

public readonly struct TextHudElement : IHudElement
{
    private readonly ITextHudElementContent _content;
    private readonly Vector2 _position;
    private readonly Size _size;
    private readonly Color _color;
    private readonly Vector2 _scale;
    private readonly IFont _font;
    private readonly HorizontalAlign _alignX;
    private readonly VerticalAlign _alignY;
    private readonly PositioningMode _positioningMode;

    public IHudElementContent? Content => _content;

    public TextHudElement(ITextHudElementContent text, Vector2 position, Size size, Color? color = null, Vector2? scale = null, IFont? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top, PositioningMode positioningMode = PositioningMode.Relative)
    {
        _content = text;
        _position = position;
        _size = size;
        _color = color ?? Color.White;
        _scale = scale ?? new Vector2(1.0f, 1.0f);
        _font = font ?? BuildInFonts.Default;
        _alignX = alignX;
        _alignY = alignY;
        _positioningMode = positioningMode;
    }

    public TextHudElement(string text, Vector2 position, Size size, Color? color = null, Vector2? scale = null, IFont? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top, PositioningMode positioningMode = PositioningMode.Relative) : this(new ConstantTextHudElementContent(text), position, size, color, scale, font, alignX, alignY, positioningMode) { }

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
            ["scale"] = LuaValue.ArrayFromVector(_scale),
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

public readonly struct ConstantNumberHudElementContent : ITextHudElementContent
{
    public float Content { get; }

    public ConstantNumberHudElementContent(float content)
    {
        Content = content;
    }

    public LuaValue CreateLuaValue(object? state) => new LuaTable
    {
        ["type"] = HudElementContentType.ConstantNumber,
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
        ["value"] = HudElementContentComputedType.VehicleSpeedText
    };
}
public readonly struct CurrentVehicleSpeedNumberHudElementContent : ITextHudElementContent
{
    private readonly float _multiplier;

    public CurrentVehicleSpeedNumberHudElementContent(float multiplier = 1)
    {
        _multiplier = multiplier;
    }
    public LuaValue CreateLuaValue(object? state) => new LuaTable
    {
        ["type"] = HudElementContentType.Computed,
        ["multiplier"] = _multiplier,
        ["value"] = HudElementContentComputedType.VehicleSpeedNummber
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

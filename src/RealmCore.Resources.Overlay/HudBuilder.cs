using RealmCore.Resources.Assets.Interfaces;
using RealmCore.Resources.Base.Extensions;
using RealmCore.Resources.Overlay.Builders;
using RealmCore.Resources.Overlay.Builders.Interfaces;
using RealmCore.Resources.Overlay.ConstructionInfos;
using RealmCore.Resources.Overlay.Enums;
using RealmCore.Resources.Overlay.Interfaces;
using SlipeServer.Packets.Definitions.Lua;
using System.Linq.Expressions;

namespace RealmCore.Resources.Overlay;

internal class HudBuilder<TState> : IHudBuilder<TState>
{
    private readonly List<LuaValue> _luaValues = new();
    private readonly TState _state;
    private readonly IAssetsService _assetsService;
    private readonly Vector2 _screenSize;
    public float Right => _screenSize.X;
    public float Bottom => _screenSize.Y;
    private int _id = 0;

    internal IEnumerable<LuaValue> HudElementsDefinitions => _luaValues;
    public Action<DynamicHudElement>? DynamicHudComponentAdded { get; set; }

    public HudBuilder(TState? defaultState, IAssetsService assetsService, Vector2 screenSize)
    {
        _state = defaultState ?? default;
        _assetsService = assetsService;
        _screenSize = screenSize;
    }

    public ITextAndHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        return AddText(b => b.WithText(text).WithPosition(position).WithSize(size).WithColor(color ?? Color.White).WithFont(font).WithHorizontalAlign(alignX).WithVerticalAlign(alignY));
    }

    public void AddLuaValue(LuaValue luaValue, bool atTheBeginning = false)
    {
        if(atTheBeginning)
        {
            _luaValues.Insert(0, luaValue);
        }
        else
        {
            _luaValues.Add(luaValue);
        }
    }

    public void AddLuaValue(TextConstructionInfo textConstructionInfo, bool atTheBeginning = false)
    {
        var luaValue = textConstructionInfo.AsLuaValue();
        if (atTheBeginning)
        {
            _luaValues.Insert(0, luaValue);
        }
        else
        {
            _luaValues.Add(luaValue);
        }
    }

    public int AllocateId() => Interlocked.Increment(ref _id);

    public ITextAndHudBuilder<TState> AddText(Action<ITextHudBuilder<TState>> textBuilderCallback)
    {
        var builder = new TextHudBuilder<TState>(AllocateId(), _state, _assetsService)
        {
            DynamicHudComponentAdded = DynamicHudComponentAdded
        };

        try
        {
            textBuilderCallback(builder);
        }
        finally
        {
            builder.DynamicHudComponentAdded = null;
        }
        var textConstructionInfo = builder.Build();
        AddLuaValue(textConstructionInfo);
        return new TextAndHudBuilder<TState>(this, textConstructionInfo);
    }

    public ITextAndHudBuilder<TState> AddText(Expression<Func<TState, string>> exp, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        return AddText(b => b.WithText(exp).WithPosition(position).WithSize(size).WithScale(scale).WithColor(color ?? Color.White).WithFont(font).WithHorizontalAlign(alignX).WithVerticalAlign(alignY));
    }

    public ITextAndHudBuilder<TState> AddVehicleSpeed(Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        return AddText(b => b.WithComputedValue(ComputedValueType.VehicleSpeed).WithPosition(position).WithSize(size).WithScale(scale).WithColor(color ?? Color.White).WithFont(font).WithHorizontalAlign(alignX).WithVerticalAlign(alignY));
    }

    public ITextAndHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, IFont? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        if (font == null)
            throw new ArgumentNullException(nameof(font));

        return AddText(b => b.WithText(text).WithPosition(position).WithSize(size).WithScale(scale).WithColor(color ?? Color.White).WithFont(font).WithHorizontalAlign(alignX).WithVerticalAlign(alignY));
    }

    public IHudBuilder<TState> AddRectangle(Vector2 position, Size size, Color color)
    {
        double luaColor = color.ToLuaColor();
        AddLuaValue(new LuaValue(new LuaValue[] { "rectangle", AllocateId(), position.X, position.Y, size.Width, size.Height, luaColor }));
        return this;
    }
}

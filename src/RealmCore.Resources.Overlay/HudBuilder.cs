using RealmCore.Resources.Base.Extensions;
using RealmCore.Resources.Overlay.Builders;
using RealmCore.Resources.Overlay.ConstructionInfos;
using RealmCore.Resources.Overlay.Interfaces;
using SlipeServer.Packets.Definitions.Lua;

namespace RealmCore.Resources.Overlay;

internal enum AddElementLocation
{
    Default,
    AtTheBeginning,
    BeforeLastElement
}

internal sealed class HudBuilder<TState> : IHudBuilder<TState>
{
    private readonly List<LuaValue> _luaValues = [];
    private readonly TState _state;
    private readonly IAssetsService _assetsService;
    private int _id = 0;

    internal IEnumerable<LuaValue> HudElementsDefinitions => _luaValues;
    public Action<DynamicHudElement>? DynamicHudElementAdded { get; set; }

    public HudBuilder(TState defaultState, IAssetsService assetsService)
    {
        _state = defaultState;
        _assetsService = assetsService;
    }

    public ITextAndHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top)
    {
        return AddText(b => b.WithText(text).WithPosition(position).WithSize(size).WithColor(color ?? Color.White).WithFont(font).WithHorizontalAlign(alignX).WithVerticalAlign(alignY));
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
    
    internal void AddLuaValue(TextConstructionInfo textConstructionInfo, AddElementLocation addElementLocation = AddElementLocation.Default)
    {
        var luaValue = textConstructionInfo.AsLuaValue();
        AddLuaValue(luaValue, addElementLocation);
    }

    public int AllocateId() => Interlocked.Increment(ref _id);

    public ITextAndHudBuilder<TState> AddText(Action<ITextHudBuilder<TState>> textBuilderCallback)
    {
        var builder = new TextHudBuilder<TState>(AllocateId(), _state, _assetsService)
        {
            DynamicHudElementAdded = DynamicHudElementAdded
        };

        try
        {
            textBuilderCallback(builder);
        }
        finally
        {
            builder.DynamicHudElementAdded = null;
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

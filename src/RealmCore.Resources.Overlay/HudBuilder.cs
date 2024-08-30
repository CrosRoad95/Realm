namespace RealmCore.Resources.Overlay;

internal sealed class HudBuilder : IHudBuilder
{
    private readonly List<LuaValue> _luaValues = [];
    private readonly object? _state;
    private readonly IAssetsService _assetsService;
    private int _id = 0;

    internal IEnumerable<LuaValue> HudElementsDefinitions => _luaValues;
    public Action<DynamicHudElement>? DynamicHudElementAdded { get; set; }

    public HudBuilder(IAssetsService assetsService, object? defaultState = null)
    {
        _assetsService = assetsService;
        _state = defaultState;
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

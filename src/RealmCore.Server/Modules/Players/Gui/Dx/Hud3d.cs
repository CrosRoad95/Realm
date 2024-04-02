namespace RealmCore.Server.Modules.Players.Gui.Dx;

public abstract class Hud3dBase : IDisposable
{
    private static int _idCounter = 0;
    protected readonly int _id;

    public string Id => _id.ToString();

    public Action<Hud3dBase, int, Dictionary<int, object?>>? StateChanged;
    public Action<Hud3dBase, int>? Removed;

    public Hud3dBase()
    {
        Interlocked.Increment(ref _idCounter);
        _id = _idCounter;
    }

    protected void SetHud3dState(int id, Dictionary<int, object?> state) => StateChanged?.Invoke(this, id, state);

    internal abstract void BuildHud(IOverlayService overlayService);

    public void Dispose()
    {
        Removed?.Invoke(this, _id);
    }
}

public class Hud3d<TState> : Hud3dBase where TState : class
{
    private readonly Action<IHudBuilder<TState>, IHudBuilderContext> _hudBuilderCallback;
    private readonly List<DynamicHudElement> _dynamicHudElements = [];
    private readonly TState? _state;

    internal IEnumerable<DynamicHudElement> DynamicHudElements => _dynamicHudElements;

    public Vector3 Position { get; }

    public Hud3d(Action<IHudBuilder<TState>, IHudBuilderContext> hudBuilderCallback, Vector3 position, TState? defaultState = null) : base()
    {
        _hudBuilderCallback = hudBuilderCallback;
        Position = position;
        _state = defaultState;
    }

    public void UpdateState(Action<TState> callback)
    {
        if (_state == null || _dynamicHudElements == null || _dynamicHudElements.Count == 0)
            throw new HudException("Hud3d has no state");

        callback(_state);
        Dictionary<int, object?> stateChange = [];
        foreach (var item in _dynamicHudElements)
        {
            var objectValue = item.PropertyInfo.GetValue(_state);
            stateChange.Add(item.Id, objectValue);
        }

        if (stateChange.Count != 0)
            SetHud3dState(_id, stateChange);
    }

    private void HandleDynamicHudElementAdded(DynamicHudElement dynamicHudElement)
    {
        _dynamicHudElements.Add(dynamicHudElement);
    }

    internal override void BuildHud(IOverlayService overlayService)
    {
        overlayService.CreateHud3d(_id.ToString(), (builder, context) =>
        {
            builder.DynamicHudElementAdded = HandleDynamicHudElementAdded;
            try
            {
                _hudBuilderCallback(builder, context);
            }
            finally
            {
                builder.DynamicHudElementAdded = null;
            }
        }, _state, Position);
    }
}

public class Hud3d : Hud3d<object>
{
    public Hud3d(Action<IHudBuilder<object>, IHudBuilderContext> hudBuilderCallback, Vector3 position) : base(hudBuilderCallback, position)
    {
    }
}


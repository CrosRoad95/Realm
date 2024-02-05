namespace RealmCore.Server.Components.Common;

public abstract class Hud3dComponentBase : ComponentLifecycle
{
    private static int _idCounter = 0;
    protected readonly int _id;

    public string Id => _id.ToString();

    public Action<Hud3dComponentBase, int, Dictionary<int, object?>>? StateChanged;
    public Action<Hud3dComponentBase, int>? Removed;

    public Hud3dComponentBase()
    {
        Interlocked.Increment(ref _idCounter);
        _id = _idCounter;
    }

    protected void SetHud3dState(int id, Dictionary<int, object?> state) => StateChanged?.Invoke(this, id, state);

    public override void Detach()
    {
        Removed?.Invoke(this, _id);
    }

    internal abstract void BuildHud(IOverlayService overlayService);
}

public class Hud3dComponent<TState> : Hud3dComponentBase where TState : class
{
    private readonly Action<IHudBuilder<TState>> _hudBuilderCallback;
    private readonly List<DynamicHudElement> _dynamicHudComponents = new();
    private readonly TState? _state;

    internal IEnumerable<DynamicHudElement> DynamicHudComponents => _dynamicHudComponents;

    public Vector3 Position { get; }

    public Hud3dComponent(Action<IHudBuilder<TState>> hudBuilderCallback, Vector3 position, TState? defaultState = null) : base()
    {
        _hudBuilderCallback = hudBuilderCallback;
        Position = position;
        _state = defaultState;
    }

    public void UpdateState(Action<TState> callback)
    {
        if (_state == null || _dynamicHudComponents == null || _dynamicHudComponents.Count == 0)
            throw new HudException("Hud3d has no state");

        callback(_state);
        Dictionary<int, object?> stateChange = new();
        foreach (var item in _dynamicHudComponents)
        {
            var objectValue = item.PropertyInfo.GetValue(_state);
            stateChange.Add(item.ComponentId, objectValue);
        }

        if (stateChange.Count != 0)
            SetHud3dState(_id, stateChange);
    }

    private void HandleDynamicHudComponentAdded(DynamicHudElement dynamicHudComponent)
    {
        _dynamicHudComponents.Add(dynamicHudComponent);
    }

    internal override void BuildHud(IOverlayService overlayService)
    {
        overlayService.CreateHud3d(_id.ToString(), e =>
        {
            e.DynamicHudComponentAdded = HandleDynamicHudComponentAdded;
            try
            {
                _hudBuilderCallback(e);
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                e.DynamicHudComponentAdded = null;
            }
        }, _state, Position);
    }
}

public class Hud3dComponent : Hud3dComponent<object>
{
    public Hud3dComponent(Action<IHudBuilder<object>> hudBuilderCallback, Vector3 position) : base(hudBuilderCallback, position)
    {
    }
}


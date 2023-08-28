namespace RealmCore.Server.Components.Common;

public class Hud3dComponentBase : Component
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

    protected override void Detach()
    {
        Removed?.Invoke(this, _id);
    }

    internal void BuildHud(IOverlayService overlayService)
    {
        throw new NotImplementedException();
    }
}

public class Hud3dComponent<TState> : Hud3dComponentBase where TState : class
{
    private readonly Action<IHudBuilder<TState>> _hudBuilderCallback;
    private readonly List<DynamicHudComponent> _dynamicHudComponents = new();
    private readonly TState? _state;
    private readonly Vector3 _offset;

    public Vector3 Offset => _offset;
    internal IEnumerable<DynamicHudComponent> DynamicHudComponents => _dynamicHudComponents;

    public Hud3dComponent(Action<IHudBuilder<TState>> hudBuilderCallback, TState? defaultState = null, Vector3? offset = null) : base()
    {
        _hudBuilderCallback = hudBuilderCallback;
        _state = defaultState;
        _offset = offset ?? Vector3.Zero;
    }

    public void UpdateState(Action<TState> callback)
    {
        ThrowIfDisposed();
        if (_state == null || _dynamicHudComponents == null || !_dynamicHudComponents.Any())
            throw new HudException("Hud3d has no state");

        callback(_state);
        Dictionary<int, object?> stateChange = new();
        foreach (var item in _dynamicHudComponents)
        {
            var objectValue = item.PropertyInfo.GetValue(_state);
            stateChange.Add(item.ComponentId, objectValue);
        }

        if (stateChange.Any())
            SetHud3dState(_id, stateChange);
    }

    private void HandleDynamicHudComponentAdded(DynamicHudComponent dynamicHudComponent)
    {
        _dynamicHudComponents.Add(dynamicHudComponent);
    }

    internal void BuildHud(IOverlayService overlayService)
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
        }, _state, Entity.Transform.Position + _offset);
    }
}

public class Hud3dComponent : Hud3dComponent<object>
{
    public Hud3dComponent(Action<IHudBuilder<object>> hudBuilderCallback, Vector3? offset = null) : base(hudBuilderCallback, null, offset)
    {
    }
}


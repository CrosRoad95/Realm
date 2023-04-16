using RealmCore.Resources.Overlay;

namespace RealmCore.Server.Components.Common;

public class Hud3dComponent<TState> : Component where TState : class
{
    [Inject]
    private IOverlayService OverlayService { get; set; } = default!;

    private static int _idCounter = 0;
    private readonly int _id = _idCounter++;
    private readonly Action<IHudBuilder<TState>> _hudBuilderCallback;
    private readonly List<DynamicHudComponent> _dynamicHudComponents = new();
    private readonly TState? _state;
    private readonly Vector3 _offset;

    public int Id => _id;
    public Vector3 Offset => _offset;
    internal IEnumerable<DynamicHudComponent> DynamicHudComponents => _dynamicHudComponents;

    public Hud3dComponent(Action<IHudBuilder<TState>> hudBuilderCallback, TState? defaultState = null, Vector3? offset = null)
    {
        _hudBuilderCallback = hudBuilderCallback;
        _state = defaultState;
        _offset = offset ?? Vector3.Zero;
    }

    public void UpdateState(Action<TState> callback)
    {
        ThrowIfDisposed();
        if (_state == null || _dynamicHudComponents == null || !_dynamicHudComponents.Any())
            throw new Exception("Hud3d has no state");

        callback(_state);
        Dictionary<int, object?> stateChange = new();
        foreach (var item in _dynamicHudComponents)
        {
            var objectValue = item.PropertyInfo.GetValue(_state);
            stateChange.Add(item.ComponentId, objectValue);
        }

        if (stateChange.Any())
            OverlayService.SetHud3dState(_id.ToString(), stateChange);
    }

    private void HandleDynamicHudComponentAdded(DynamicHudComponent dynamicHudComponent)
    {
        _dynamicHudComponents.Add(dynamicHudComponent);
    }

    protected override void Load()
    {
        OverlayService.CreateHud3d(_id.ToString(), e =>
        {
            e.DynamicHudComponentAdded = HandleDynamicHudComponentAdded;
            try
            {
                _hudBuilderCallback(e);
            }
            catch(Exception ex)
            {
                throw;
            }
            finally
            {
                e.DynamicHudComponentAdded = null;
            }
        }, _state, Entity.Transform.Position + _offset);

        base.Load();
    }

    public override void Dispose()
    {
        OverlayService.RemoveHud3d(_id.ToString());
    }
}

public class Hud3dComponent : Hud3dComponent<object>
{
    public Hud3dComponent(Action<IHudBuilder<object>> hudBuilderCallback, Vector3? offset = null) : base(hudBuilderCallback, null, offset)
    {
    }
}


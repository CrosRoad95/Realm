using Realm.Resources.Overlay.Interfaces;

namespace Realm.Domain.Components.Common;

public class Hud3dComponent<TState> : Component where TState : class
{
    [Inject]
    private IOverlayService OverlayService { get; set; } = default!;

    private static int _idCounter = 0;
    private readonly int _id = _idCounter++;
    private readonly Action<IHudBuilder<TState>> _hudBuilderCallback;
    private readonly List<(int, PropertyInfo)> _dynamicHudComponents = new();
    private readonly TState _state;
    private readonly Vector3? _offset;

    public Hud3dComponent(Action<IHudBuilder<TState>> hudBuilderCallback, TState defaultState, Vector3? offset = null)
    {
        _hudBuilderCallback = hudBuilderCallback;
        _state = defaultState;
        _offset = offset;
    }

    public void UpdateState(Action<TState> callback)
    {
        ThrowIfDisposed();
        if (_state == null || _dynamicHudComponents == null || !_dynamicHudComponents.Any())
            throw new Exception("Hud3d has no state");

        callback(_state);
        Dictionary<int, object> stateChange = new();
        foreach (var item in _dynamicHudComponents)
        {
            var value = item.Item2.GetValue(_state).ToString();
            stateChange.Add(item.Item1, value);
        }

        if (stateChange.Any())
            OverlayService.SetHud3dState(_id.ToString(), stateChange);
    }

    protected override void Load()
    {
        List<(int, PropertyInfo)> dynamicHudComponents = new();

        var HandleDynamicHudComponentAdded = (int id, PropertyInfo propertyInfo) =>
        {
            dynamicHudComponents.Add((id, propertyInfo));
        };

        OverlayService.CreateHud3d(_id.ToString(), e =>
        {
            e.DynamicHudComponentAdded += HandleDynamicHudComponentAdded;
            _hudBuilderCallback(e);
            e.DynamicHudComponentAdded -= HandleDynamicHudComponentAdded;
        }, _state, Entity.Transform.Position + (_offset ?? Vector3.Zero));

        _dynamicHudComponents.AddRange(dynamicHudComponents);
        base.Load();
    }

    public override void Dispose()
    {
        OverlayService.RemoveHud3d(_id.ToString());
    }
}

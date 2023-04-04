using Realm.Domain.Concepts;
using Realm.Resources.Overlay.Interfaces;

namespace Realm.Domain.Components.Players;

[ComponentUsage(true)]
public abstract class HudComponent<TState> : Component where TState : class
{
    [Inject]
    private IOverlayService OverlayService { get; set; } = default!;

    private readonly string _id = Guid.NewGuid().ToString();
    private Hud<TState> _hud = default!;
    private readonly TState _defaultState;
    private readonly Vector2? _offset;
    private bool _visible;

    public bool Visible
    {
        get
        {
            ThrowIfDisposed();
            return _visible;
        }
        set
        {
            ThrowIfDisposed();
            if (_visible != value)
            {
                _visible = value;
                _hud.SetVisible(_visible);
            }
        }
    }

    public Vector2 Position
    {
        get
        {
            ThrowIfDisposed();
            return _hud.Position;
        }
        set
        {
            ThrowIfDisposed();
            _hud.Position = value;
        }
    }

    public HudComponent(TState defaultState, Vector2? offset = null)
    {
        _defaultState = defaultState;
        _offset = offset;
    }

    protected override void Load()
    {
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        List<(int, PropertyInfo)> dynamicHudComponents = new();

        var HandleDynamicHudComponentAdded = (int id, PropertyInfo propertyInfo) =>
        {
            dynamicHudComponents.Add((id, propertyInfo));
        };

        OverlayService.CreateHud(playerElementComponent.Player, _id, e =>
        {
            e.DynamicHudComponentAdded += HandleDynamicHudComponentAdded;
            Build(e);
            e.DynamicHudComponentAdded -= HandleDynamicHudComponentAdded;
        }, playerElementComponent.ScreenSize, _offset, _defaultState);
        _hud = new Hud<TState>(_id, playerElementComponent.Player, OverlayService, _offset, _defaultState, dynamicHudComponents);
        Visible = true;
    }

    protected void UpdateState(Action<TState> callback)
    {
        _hud.UpdateState(callback);
    }

    protected abstract void Build(IHudBuilder<TState> hudBuilderCallback);

    public override void Dispose()
    {
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        OverlayService.RemoveHud(playerElementComponent.Player, _id);
    }
}

[ComponentUsage(true)]
public abstract class HudComponent : HudComponent<object>
{
    public HudComponent(Vector2? offset = null) : base(new(), offset)
    {

    }
}

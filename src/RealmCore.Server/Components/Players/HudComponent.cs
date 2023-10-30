namespace RealmCore.Server.Components.Players;

internal interface IStatefulHudComponent : IComponent
{
    internal string Id { get; }

    internal void BuildHud(IOverlayService overlayService);
}

public abstract class HudComponent<TState> : Component, IStatefulHudComponent where TState : class, new()
{
    private readonly string _id = Guid.NewGuid().ToString();
    private Hud<TState> _hud = default!;
    private readonly TState _defaultState;
    private readonly Vector2? _offset;
    private bool _visible;

    public string Id => _id;
    public bool Visible
    {
        get
        {
            return _visible;
        }
        set
        {
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
            return _hud.Position;
        }
        set
        {
            _hud.Position = value;
        }
    }

    public HudComponent(TState defaultState, Vector2? offset = null)
    {
        _defaultState = defaultState;
        var initialState = GetInitialState();
        if (initialState != null)
            _defaultState = initialState;
        _offset = offset;
    }

    public HudComponent(Vector2? offset = null) : this(new(), offset) { }

    protected void UpdateState(Action<TState> callback)
    {
        if (_hud == null)
        {
            callback(_defaultState);
        }
        else
        {
            _hud.UpdateState(callback);
        }
    }

    protected virtual TState? GetInitialState()
    {
        return null;
    }

    protected abstract void Build(IHudBuilder<TState> hudBuilderCallback);

    protected virtual void HudCreated() { }

    void IStatefulHudComponent.BuildHud(IOverlayService overlayService)
    {
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        List<DynamicHudComponent> dynamicHudComponents = new();

        overlayService.CreateHud(playerElementComponent, _id, e =>
        {
            e.DynamicHudComponentAdded = dynamicHudComponents.Add;
            try
            {
                Build(e);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                e.DynamicHudComponentAdded = null;
            }
        }, playerElementComponent.ScreenSize, _offset, _defaultState);

        _hud = new Hud<TState>(_id, playerElementComponent, overlayService, _offset, _defaultState, dynamicHudComponents);
        Visible = true;
        HudCreated();
    }
}

[ComponentUsage(true)]
public abstract class HudComponent : HudComponent<object>
{
    public HudComponent(Vector2? offset = null) : base(new(), offset)
    {

    }
}

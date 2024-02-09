namespace RealmCore.Server.Modules.Players.Gui.Dx;

public interface IHudLayer
{
    bool Visible { get; set; }
    Vector2 Offset { get; set; }
    internal string Id { get; }
    internal void BuildHud(IOverlayService overlayService, RealmPlayer player);
}

public abstract class HudLayer<TState> : IHudLayer where TState : class, new()
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

    public Vector2 Offset
    {
        get
        {
            return _hud.Offset;
        }
        set
        {
            _hud.Offset = value;
        }
    }

    public HudLayer(TState defaultState, Vector2? offset = null)
    {
        _defaultState = defaultState;
        var initialState = GetInitialState();
        if (initialState != null)
            _defaultState = initialState;
        _offset = offset;
    }

    public HudLayer(Vector2? offset = null) : this(new(), offset) { }

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

    public void BuildHud(IOverlayService overlayService, RealmPlayer player)
    {
        List<DynamicHudElement> dynamicHudElement = [];

        overlayService.CreateHud(player, _id, e =>
        {
            e.DynamicHudElementAdded = dynamicHudElement.Add;
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
                e.DynamicHudElementAdded = null;
            }
        }, player.ScreenSize, _offset, _defaultState);

        _hud = new Hud<TState>(_id, player, overlayService, _offset, _defaultState, dynamicHudElement);
        Visible = true;
        HudCreated();
    }
}

public abstract class HudLayer : HudLayer<object>
{
    public HudLayer(Vector2? offset = null) : base(new(), offset)
    {

    }
}

namespace RealmCore.Server.Modules.Players.Gui.Dx;

public interface IHudLayer : IDisposable
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
    private TState _defaultState;
    private Vector2 _offset;
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
        get => _offset;
        set
        {
            _offset = value;
            _hud.SetPosition(value);
        }
    }

    public HudLayer(TState defaultState, Vector2? offset = null)
    {
        _defaultState = defaultState;
        _offset = offset ?? Vector2.Zero;
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

    protected bool SetPosition(int elementId, Vector2 position)
    {
        return _hud.SetPosition(elementId, position);
    }
    
    protected bool SetSize(int elementId, Size size)
    {
        return _hud.SetSize(elementId, size);
    }
    
    protected bool SetVisible(int elementId, bool visible)
    {
        return _hud.SetVisible(elementId, visible);
    }
    
    protected void SetContent(int elementId, IHudElementContent content)
    {
        _hud.SetContent(elementId, content);
    }

    protected abstract void Build(IHudBuilder hudBuilderCallback, IHudBuilderContext hudBuilderContext);

    protected virtual void HudCreated() { }

    public void BuildHud(IOverlayService overlayService, RealmPlayer player)
    {
        var initialState = GetInitialState();
        if (initialState != null)
            _defaultState = initialState;

        List<DynamicHudElement> dynamicHudElement = [];

        overlayService.CreateHud(player, _id, (builder, context) =>
        {
            builder.DynamicHudElementAdded = dynamicHudElement.Add;
            try
            {
                Build(builder, context);
            }
            finally
            {
                builder.DynamicHudElementAdded = null;
            }
        }, player.ScreenSize, _offset, _defaultState);

        _hud = new Hud<TState>(_id, player, overlayService, _offset, _defaultState, dynamicHudElement);
        Visible = true;
        HudCreated();
    }

    protected virtual ITextHudElementContent CreateStatePropertyTextHudElement<TProperty>(Expression<Func<TState, TProperty>> expression)
    {
        return StatePropertyTextHudElementContent.Create(expression);
    }

    public virtual void Dispose() { }
}

public abstract class HudLayer : HudLayer<object>
{
    public HudLayer(Vector2? offset = null) : base(new(), offset)
    {

    }
}

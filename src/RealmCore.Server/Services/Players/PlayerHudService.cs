namespace RealmCore.Server.Services.Players;

public interface IHudLayer
{
    bool Visible { get; set; }
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
        List<DynamicHudElement> dynamicHudComponents = new();

        overlayService.CreateHud(player, _id, e =>
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
        }, player.ScreenSize, _offset, _defaultState);

        _hud = new Hud<TState>(_id, player, overlayService, _offset, _defaultState, dynamicHudComponents);
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

public interface IPlayerHudService : IPlayerService
{
    IReadOnlyList<IHudLayer> Layers { get; }

    event Action<IPlayerHudService, IHudLayer>? LayerCreated;
    event Action<IPlayerHudService, IHudLayer>? LayerRemoved;

    bool AddLayer(IHudLayer hudLayer);
    bool RemoveLayer(IHudLayer hudLayer);
}

internal sealed class PlayerHudService : IPlayerHudService
{
    public RealmPlayer Player { get; private set; }

    private readonly List<IHudLayer> _hudLayers = [];
    private readonly object _lock = new();

    public event Action<IPlayerHudService, IHudLayer>? LayerCreated;
    public event Action<IPlayerHudService, IHudLayer>? LayerRemoved;

    public IReadOnlyList<IHudLayer> Layers
    {
        get
        {
            lock (_lock)
            {
                return [.._hudLayers];
            }
        }
    }

    public PlayerHudService(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public bool AddLayer(IHudLayer hudLayer)
    {
        lock (_lock)
        {
            if (_hudLayers.Contains(hudLayer))
                return false;
            _hudLayers.Add(hudLayer);
        }

        LayerCreated?.Invoke(this, hudLayer);
        return true;
    }

    public bool RemoveLayer(IHudLayer hudLayer)
    {
        lock (_lock)
        {
            if (_hudLayers.Remove(hudLayer))
                return false;
        }

        LayerRemoved?.Invoke(this, hudLayer);
        return false;
    }
}

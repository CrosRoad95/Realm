using Microsoft.Extensions.DependencyInjection;

namespace RealmCore.Server.Modules.World;

public interface IWorldHud : IDisposable
{
    bool Visible { get; set; }
    Vector3 Position { get; set; }
    internal string Id { get; }
    internal void BuildHud(IOverlayService overlayService);
}
public abstract class WorldHud<TState> : IWorldHud where TState : class, new()
{
    private readonly string _id = Guid.NewGuid().ToString();
    private Hud3d<TState> _hud = default!;
    private TState _defaultState;
    private Vector3 _position;
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
                if(_hud != null)
                    _hud.SetVisible(_visible);
            }
        }
    }

    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            if (_hud != null)
                _hud.SetPosition(value);
        }
    }

    public WorldHud(TState defaultState, Vector3? position = null)
    {
        _defaultState = defaultState;
        _position = position ?? Vector3.Zero;
    }

    public WorldHud(Vector3? offset = null) : this(new(), offset) { }

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

    protected abstract void Build(IHudBuilder hudBuilderCallback, IHudBuilderContext hudBuilderContext);

    protected virtual void HudCreated() { }

    public void BuildHud(IOverlayService overlayService)
    {
        var initialState = GetInitialState();
        if (initialState != null)
            _defaultState = initialState;

        List<DynamicHudElement> dynamicHudElement = [];

        overlayService.CreateHud3d(_id, (builder, context) =>
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
        }, _position, _defaultState);

        _hud = new Hud3d<TState>(_id, overlayService, _position, _defaultState, dynamicHudElement);
        _hud.SetPosition(_position);
        if (_visible)
            _hud.SetVisible(true);

        HudCreated();
    }

    protected virtual ITextHudElementContent CreateStatePropertyTextHudElement<TProperty>(Expression<Func<TState, TProperty>> expression)
    {
        return StatePropertyTextHudElementContent.Create(expression);
    }

    public virtual void Dispose() { }
}

public sealed class WorldHudService
{
    private readonly object _lock = new();
    private readonly IOverlayService _overlayService;
    private readonly IServiceProvider _serviceProvider;
    private List<IWorldHud> _worldHuds = [];

    public WorldHudService(IOverlayService overlayService, IServiceProvider serviceProvider)
    {
        _overlayService = overlayService;
        _serviceProvider = serviceProvider;
    }

    public TWorldHud Create<TWorldHud>(Vector3 position, params object[] parameters) where TWorldHud : IWorldHud
    {
        lock (_lock)
        {
            var worldHud = ActivatorUtilities.CreateInstance<TWorldHud>(_serviceProvider, parameters);
            worldHud.Position = position;
            worldHud.BuildHud(_overlayService);
            _worldHuds.Add(worldHud);
            return worldHud;
        }
    }

    public bool Remove(IWorldHud worldHud)
    {
        lock (_lock)
        {
            if (_worldHuds.Remove(worldHud))
            {
                return true;
            }
            return false;
        }
    }
}
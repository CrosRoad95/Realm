using SlipeServer.Server.Elements;

namespace RealmCore.Server.Modules.Players.Gui.Dx;

internal abstract class HudBase<TState> : IHud<TState> where TState : class
{
    protected readonly IOverlayService _overlayService;
    private readonly TState? _state;
    private readonly List<DynamicHudElement>? _dynamicHudElements;
    private bool _disposed = false;

    public event Action<IHud>? Disposed;

    public string Id { get; }

    public HudBase(string id, IOverlayService overlayService, TState? state = default, List<DynamicHudElement>? dynamicHudElements = null)
    {
        Id = id;
        _overlayService = overlayService;
        _state = state;
        _dynamicHudElements = dynamicHudElements;
    }

    public abstract void SetVisible(bool visible);
    protected abstract void SetState(string hudId, Dictionary<int, object?> stateChange);

    public void UpdateState(Action<TState> callback)
    {
        ThrowIfDisposed();
        if (_state == null || _dynamicHudElements == null || _dynamicHudElements.Count == 0)
            throw new Exception("Hud has no state");

        callback(_state);
        Dictionary<int, object?> stateChange = [];
        foreach (var item in _dynamicHudElements)
        {
            var value = item.Factory.DynamicInvoke(_state);
            stateChange.Add(item.Id, value);
        }

        if (stateChange.Count != 0)
            SetState(Id, stateChange);
    }

    public abstract bool SetSize(int elementId, Vector2 size);

    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public virtual void Dispose()
    {
        ThrowIfDisposed();
        _disposed = true;
        Disposed?.Invoke(this);
    }
}

internal class Hud<TState> : HudBase<TState> where TState : class
{
    private Vector2 _position;
    private readonly Player _player;

    public Hud(string id, Player player, IOverlayService overlayService, Vector2? position = null, TState? state = default, List<DynamicHudElement>? dynamicHudElements = null) : base(id, overlayService, state, dynamicHudElements)
    {
        if (position != null)
            _position = position.Value;
        _player = player;
    }

    public override void SetVisible(bool visible)
    {
        ThrowIfDisposed();
        _overlayService.SetHudVisible(_player, Id, visible);
    }
    
    protected override void SetState(string hudId, Dictionary<int, object?> stateChange)
    {
        ThrowIfDisposed();
        _overlayService.SetHudState(_player, Id, stateChange);
    }

    public void SetPosition(Vector2 position)
    {
        ThrowIfDisposed();
        _overlayService.SetHudPosition(_player, Id, position);
    }

    public override bool SetSize(int elementId, Vector2 size)
    {
        _overlayService.SizeChanged(_player, Id, elementId, size);
        return true;
    }
}

internal class Hud3d<TState> : HudBase<TState> where TState : class
{
    private Vector3 _position;

    public Hud3d(string id, IOverlayService overlayService, Vector3? position = null, TState? state = default, List<DynamicHudElement>? dynamicHudElements = null) : base(id, overlayService, state, dynamicHudElements)
    {
        if (position != null)
            _position = position.Value;
    }

    public override void SetVisible(bool visible)
    {
        ThrowIfDisposed();
        _overlayService.Set3dHudVisible(Id, visible);
    }

    protected override void SetState(string hudId, Dictionary<int, object?> stateChange)
    {
        ThrowIfDisposed();
        _overlayService.SetHud3dState(Id, stateChange);
    }

    public void SetPosition(Vector3 position)
    {
        ThrowIfDisposed();
        _overlayService.SetHud3dPosition(Id, position);
    }

    public override bool SetSize(int elementId, Vector2 size)
    {
        throw new NotSupportedException();
    }
}

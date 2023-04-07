namespace RealmCore.Server.Concepts;

internal class Hud<TState> : IHud<TState> where TState : class
{
    private readonly Player _player;
    private readonly IOverlayService _overlayService;
    private readonly TState? _state;
    private readonly List<(int, PropertyInfo)>? _dynamicHudComponents;
    private bool _disposed = false;
    private Vector2 _position;

    public event Action<IHud>? Disposed;

    public string Name { get; }

    public Vector2 Position
    {
        get => _position; set
        {
            ThrowIfDisposed();
            _overlayService.SetHudPosition(_player, Name, value);
            _position = value;
        }
    }

    public Hud(string name, Player player, IOverlayService overlayService, Vector2? position = null, TState? state = default, List<(int, PropertyInfo)>? dynamicHudComponents = null)
    {
        Name = name;
        _player = player;
        _overlayService = overlayService;
        _state = state;
        _dynamicHudComponents = dynamicHudComponents;
        if (position != null)
            _position = Position;
    }

    public void SetVisible(bool visible)
    {
        ThrowIfDisposed();
        _overlayService.SetHudVisible(_player, Name, visible);
    }

    public void UpdateState(Action<TState> callback)
    {
        ThrowIfDisposed();
        if (_state == null || _dynamicHudComponents == null || !_dynamicHudComponents.Any())
            throw new Exception("Hud has no state");

        callback(_state);
        Dictionary<int, object> stateChange = new();
        foreach (var item in _dynamicHudComponents)
        {
            var value = item.Item2.GetValue(_state).ToString();
            stateChange.Add(item.Item1, value);
        }

        if (stateChange.Any())
            _overlayService.SetHudState(_player, Name, stateChange);
    }

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

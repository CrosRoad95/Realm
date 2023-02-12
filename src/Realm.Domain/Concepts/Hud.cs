namespace Realm.Domain.Concepts;

internal class Hud<TState> : IHud<TState> where TState : class
{
    private readonly Player _player;
    private readonly OverlayService _overlayService;
    private readonly TState? _state;

    private bool _disposed = false;
    private Vector2 _position;

    public event Action<IHud>? Disposed;

    public string Name { get; }

    public Vector2 Position { get => _position; set
        {
            ThrowIfDisposed();
            _overlayService.SetHudPosition(_player, Name, value);
            _position = value;
        }
    }

    public Hud(string name, Player player, OverlayService overlayService, Vector2? position = null, TState? state = default)
    {
        Name = name;
        _player = player;
        _overlayService = overlayService;
        _state = state;
        if (position != null)
            _position = Position;
    }

    public void SetVisible(bool visible)
    {
        ThrowIfDisposed();
        _overlayService.SetHudVisible(_player, Name, visible);
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

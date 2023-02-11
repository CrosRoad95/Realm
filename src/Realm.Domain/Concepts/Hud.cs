namespace Realm.Domain.Concepts;

internal class Hud<TState> : IHud<TState> where TState : class
{
    private readonly Player _player;
    private readonly OverlayService _overlayService;
    private readonly TState? _state;

    private bool _disposed = false;

    public event Action<IHud>? Disposed;

    public string Name { get; }

    public Vector2 Position { get; set; }

    public Hud(string name, Player player, OverlayService overlayService, Vector2? position = null, TState? state = default)
    {
        Name = name;
        _player = player;
        _overlayService = overlayService;
        _state = state;
        if (position != null)
            position = Position;
    }

    public void SetVisible(bool visible)
    {
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

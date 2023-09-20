namespace RealmCore.Server.Components.Common;

[ComponentUsage(false)]
public class FocusableComponent : Component
{
    private readonly object _lock = new();
    private readonly List<Entity> _focusedPlayers = new();
    public event Action<FocusableComponent, Entity>? PlayerFocused;
    public event Action<FocusableComponent, Entity>? PlayerLostFocus;
    public int FocusedPlayerCount
    {
        get
        {
            lock (_lock)
                return _focusedPlayers.Count;
        }
    }

    public IEnumerable<Entity> FocusedPlayer
    {
        get
        {
            lock (_lock)
            {
                foreach (var focusedPlayer in _focusedPlayers)
                {
                    yield return focusedPlayer;
                }
            }
        }
    }

    internal void AddFocusedPlayer(Entity entity)
    {
        lock (_lock)
        {
            if (!_focusedPlayers.Contains(entity))
            {
                _focusedPlayers.Add(entity);
                entity.Disposed += HandleDisposed;
                PlayerFocused?.Invoke(this, entity);
            }
        }
    }

    private void HandleDisposed(Entity entity)
    {
        lock (_lock)
        {
            if (_focusedPlayers.Remove(entity))
            {
                entity.Disposed -= HandleDisposed;
                PlayerLostFocus?.Invoke(this, entity);
            }
        }
    }

    internal void RemoveFocusedPlayer(Entity entity)
    {
        lock (_lock)
        {
            if (_focusedPlayers.Remove(entity))
            {
                entity.Disposed -= HandleDisposed;
                PlayerLostFocus?.Invoke(this, entity);
            }
        }
    }

    protected override void Detach()
    {
        lock (_lock)
        {
            foreach (var focusedPlayer in _focusedPlayers)
            {
                if (focusedPlayer.TryGetComponent(out PlayerElementComponent playerElementComponent) && playerElementComponent.FocusedEntity == Entity)
                    playerElementComponent.FocusedEntity = null;
            }
        }
        base.Detach();
    }
}

namespace RealmCore.Server.Components.Common;

[ComponentUsage(false)]
public class FocusableComponent : ComponentLifecycle
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

    public IEnumerable<Entity> FocusedPlayers
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

    internal bool AddFocusedPlayer(Entity entity)
    {
        if (entity == Entity)
            throw new InvalidOperationException(nameof(entity));

        lock (_lock)
        {
            if (!_focusedPlayers.Contains(entity))
            {
                _focusedPlayers.Add(entity);
                entity.Disposed += HandleDisposed;
                PlayerFocused?.Invoke(this, entity);
                return true;
            }
        }
        return false;
    }

    public bool RemoveFocusedPlayer(Entity entity)
    {
        if (entity == Entity)
            throw new InvalidOperationException(nameof(entity));

        lock (_lock)
        {
            if (_focusedPlayers.Remove(entity))
            {
                entity.Disposed -= HandleDisposed;
                PlayerLostFocus?.Invoke(this, entity);
                return true;
            }
        }
        return false;
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

    public override void Detach()
    {
        lock (_lock)
        {
            foreach (var focusedPlayer in _focusedPlayers)
            {
                PlayerLostFocus?.Invoke(this, focusedPlayer);
                if (focusedPlayer.TryGetComponent(out PlayerElementComponent playerElementComponent) && playerElementComponent.FocusedEntity == Entity)
                    playerElementComponent.FocusedEntity = null;
            }
        }
    }
}

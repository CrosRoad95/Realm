namespace RealmCore.Server.Components.Common;

[ComponentUsage(false)]
public class FocusableComponent : ComponentLifecycle
{
    private readonly object _lock = new();
    private readonly List<RealmPlayer> _focusedPlayers = new();
    public event Action<FocusableComponent, RealmPlayer>? PlayerFocused;
    public event Action<FocusableComponent, RealmPlayer>? PlayerLostFocus;
    public int FocusedPlayerCount
    {
        get
        {
            lock (_lock)
                return _focusedPlayers.Count;
        }
    }

    public IEnumerable<RealmPlayer> FocusedPlayers
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

    internal bool AddFocusedPlayer(RealmPlayer player)
    {
        if (player == Element)
            throw new InvalidOperationException(nameof(player));

        lock (_lock)
        {
            if (!_focusedPlayers.Contains(player))
            {
                _focusedPlayers.Add(player);
                player.Destroyed += HandleDestroyed;
                PlayerFocused?.Invoke(this, player);
                return true;
            }
        }
        return false;
    }

    public bool RemoveFocusedPlayer(RealmPlayer player)
    {
        if (player == Element)
            throw new InvalidOperationException(nameof(player));

        lock (_lock)
        {
            if (_focusedPlayers.Remove(player))
            {
                player.Destroyed -= HandleDestroyed;
                PlayerLostFocus?.Invoke(this, player);
                return true;
            }
        }
        return false;
    }

    private void HandleDestroyed(Element element)
    {
        lock (_lock)
        {
            var player = (RealmPlayer)element;
            if (_focusedPlayers.Remove(player))
            {
                element.Destroyed -= HandleDestroyed;
                PlayerLostFocus?.Invoke(this, player);
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
                if (focusedPlayer.FocusedElement == Element)
                    focusedPlayer.FocusedElement = null;
            }
        }
    }
}

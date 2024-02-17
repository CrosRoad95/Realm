namespace RealmCore.Server.Modules.Elements.Focusable;

public class FocusableRealmVehicle : RealmVehicle, IFocusableElement
{
    private readonly object _lock = new();
    private readonly List<RealmPlayer> _focusedPlayers = [];

    public event Action<Element, RealmPlayer>? PlayerFocused;
    public event Action<Element, RealmPlayer>? PlayerLostFocus;
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

    public FocusableRealmVehicle(IServiceProvider serviceProvider, ushort model, Vector3 position) : base(serviceProvider, model, position)
    {
    }

    public bool AddFocusedPlayer(RealmPlayer player)
    {
        lock (_lock)
        {
            if (!_focusedPlayers.Contains(player))
            {
                _focusedPlayers.Add(player);
                player.Destroyed += HandlePlayerDestroyed;
                PlayerFocused?.Invoke(this, player);
                return true;
            }
        }
        return false;
    }

    public bool RemoveFocusedPlayer(RealmPlayer player)
    {
        lock (_lock)
        {
            if (_focusedPlayers.Remove(player))
            {
                player.Destroyed -= HandlePlayerDestroyed;
                PlayerLostFocus?.Invoke(this, player);
                return true;
            }
        }
        return false;
    }

    private void HandlePlayerDestroyed(Element element)
    {
        lock (_lock)
        {
            var player = (RealmPlayer)element;
            if (_focusedPlayers.Remove(player))
            {
                element.Destroyed -= HandlePlayerDestroyed;
                PlayerLostFocus?.Invoke(this, player);
            }
        }
    }
}

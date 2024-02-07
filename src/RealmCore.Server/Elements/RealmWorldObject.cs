using RealmCore.Server.Concepts.Interactions;

namespace RealmCore.Server.Elements;

public class RealmWorldObject : WorldObject, IInteraction
{
    private object _lock = new();
    private RealmPlayer? _owner;
    private Interaction? _interaction;

    public RealmPlayer? Owner => _owner;

    public event Action<RealmWorldObject, RealmPlayer?>? OwnerChanged;
    public event Action<Element, Interaction?, Interaction?>? InteractionChanged;
    public Interaction? Interaction
    {
        get => _interaction; set
        {
            if (_interaction != value)
            {
                var previous = _interaction;
                _interaction = value;
                InteractionChanged?.Invoke(this, previous, value);
            }
        }
    }

    public RealmWorldObject(ObjectModel model, Vector3 position) : base(model, position)
    {
        Destroyed += HandleDestroyed;
    }

    public bool TrySetOwner(RealmPlayer player)
    {
        lock (_lock)
        {
            if (_owner != null)
                return false;

            _owner = player;
            _owner.Destroyed += HandleOwnerDestroyed;
        }
        OwnerChanged?.Invoke(this, player);
        return true;
    }

    public bool TryRemoveOwner(RealmPlayer? player)
    {
        lock (_lock)
        {
            if (_owner == null)
                return false;

            if (player != null && _owner != player)
            {
                return false;
            }

            _owner.Destroyed -= HandleOwnerDestroyed;
            _owner = null;
        }
        OwnerChanged?.Invoke(this, null);
        return true;
    }

    private void HandleOwnerDestroyed(Element obj)
    {
        Destroy();
        if (_owner != null)
        {
            _owner.Destroyed -= HandleOwnerDestroyed;
            _owner = null;
        }
    }

    private void HandleDestroyed(Element obj)
    {
        lock (_lock)
        {
            if (_owner == null)
                return;

            _owner.Destroyed += HandleOwnerDestroyed;
            _owner = null;
        }
        OwnerChanged?.Invoke(this, null);
    }
}

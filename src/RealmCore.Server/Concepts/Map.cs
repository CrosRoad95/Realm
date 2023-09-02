namespace RealmCore.Server.Concepts;

internal sealed class Map
{
    private readonly object _lock = new();
    private readonly List<WorldObject> _worldObjects;
    private readonly BoundingBox _boundingBox;
    private readonly List<Player> _createdForPlayers = new();
    public List<Player> CreatedForPlayers
    {
        get
        {
            lock(_lock)
                return new List<Player>(_createdForPlayers);
        }
    }

    internal IReadOnlyCollection<WorldObject> WorldObjects => _worldObjects.AsReadOnly();

    public BoundingBox BoundingBox => _boundingBox;

    public Map(MapIdGenerator mapIdGenerator, IEnumerable<WorldObject> worldObjects)
    {
        if (worldObjects.TryGetNonEnumeratedCount(out int count))
        {
            _worldObjects = new(count);
        }
        else
        {
            _worldObjects = new(worldObjects.Count());
        }

        Vector3 min = Vector3.Zero;
        Vector3 max = Vector3.Zero;
        foreach (var worldObject in worldObjects)
        {
            worldObject.Id = (ElementId)mapIdGenerator.GetId();
            var pos = worldObject.Position;
            if (pos.X < min.X) min.X = pos.X;
            if (pos.X > max.X) max.X = pos.X;
            if (pos.Y < min.Y) min.Y = pos.Y;
            if (pos.Y > max.Y) max.Y = pos.Y;
            if (pos.Z < min.Z) min.Z = pos.Z;
            if (pos.X > max.Z) max.Z = pos.Z;
            _worldObjects.Add(worldObject);
        }

        _boundingBox = new BoundingBox((min + max) * 0.5f, max - min);
    }

    public bool IsCreatedFor(Entity entity) => IsCreatedFor(entity.GetPlayer());
    public bool IsCreatedFor(Player player)
    {
        lock (_lock)
        {
            return _createdForPlayers.Contains(player);
        }
    }

    public bool LoadForPlayer(Entity entity) => LoadForPlayer(entity.GetPlayer());
    public bool LoadForPlayer(Player player)
    {
        lock(_lock)
        {
            if (_createdForPlayers.Contains(player))
                return false;

            _createdForPlayers.Add(player);
            player.Disconnected += HandleDisconnected;

            foreach (var worldObject in _worldObjects)
                worldObject.CreateFor(player);
            return true;
        }
    }

    private void HandleDisconnected(Player sender, PlayerQuitEventArgs e)
    {
        lock(_lock)
        {
            sender.Disconnected -= HandleDisconnected;
            _createdForPlayers.Remove(sender);
        }
    }
}

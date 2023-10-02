namespace RealmCore.Server.Concepts;

public sealed class Map : IDisposable
{
    private readonly object _lock = new();
    private readonly List<WorldObject> _worldObjects;
    private readonly BoundingBox _boundingBox;
    private readonly List<Entity> _createdForEntities = new();
    private bool _disposed = false;

    public List<Entity> CreatedForPlayers
    {
        get
        {
            ThrowIfDisposed();
            lock (_lock)
                return new List<Entity>(_createdForEntities);
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

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public bool IsCreatedFor(Entity entity)
    {
        ThrowIfDisposed();
        lock (_lock)
            return _createdForEntities.Contains(entity);
    }

    public bool LoadFor(Entity entity)
    {
        ThrowIfDisposed();
        lock (_lock)
        {
            if (_createdForEntities.Contains(entity))
                return false;

            _createdForEntities.Add(entity);
            entity.Disposed += HandleDisposed;
        }

        var player = entity.GetPlayer();
        foreach (var worldObject in _worldObjects)
            worldObject.CreateFor(player);
        return true;
    }

    private void HandleDisposed(Entity entity)
    {
        lock (_lock)
        {
            entity.Disposed -= HandleDisposed;
            _createdForEntities.Remove(entity);
        }
    }

    public bool UnloadFor(Entity entity)
    {
        ThrowIfDisposed();
        lock (_lock)
            if (!_createdForEntities.Remove(entity))
                return false;

        var player = entity.GetPlayer();
        foreach (var worldObject in _worldObjects)
            worldObject.DestroyFor(player);

        return true;
    }

    public void Dispose()
    {
        ThrowIfDisposed();
        lock (_lock)
        {
            foreach (var entity in _createdForEntities)
            {
                try
                {
                    var player = entity.GetPlayer();
                    foreach (var worldObject in _worldObjects)
                        worldObject.DestroyFor(player);
                }
                catch (Exception)
                {
                    // TODO: Probably not needed
                }
            }
            _createdForEntities.Clear();
        }
        _disposed = true;
    }
}

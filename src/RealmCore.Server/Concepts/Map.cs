namespace RealmCore.Server.Concepts;

public sealed class Map : IDisposable
{
    private readonly List<WorldObject> _worldObjects;
    private readonly BoundingBox _boundingBox;
    private bool _disposed = false;

    internal IReadOnlyCollection<WorldObject> WorldObjects => _worldObjects.AsReadOnly();

    public BoundingBox BoundingBox => _boundingBox;
    public string Name { get; }

    public Map(IElementIdGenerator elementIdGenerator, IEnumerable<WorldObject> worldObjects, string name)
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
            worldObject.Id = (ElementId)elementIdGenerator.GetId();
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
        Name = name;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public bool LoadFor(RealmPlayer player)
    {
        ThrowIfDisposed();
        foreach (var worldObject in _worldObjects)
            worldObject.CreateFor(player);
        return true;
    }

    public bool UnloadFor(RealmPlayer player)
    {
        ThrowIfDisposed();
        foreach (var worldObject in _worldObjects)
            worldObject.DestroyFor(player);

        return true;
    }

    public event Action<Map>? Disposed;
    public void Dispose()
    {
        ThrowIfDisposed();
        Disposed?.Invoke(this);

        _disposed = true;
    }
}

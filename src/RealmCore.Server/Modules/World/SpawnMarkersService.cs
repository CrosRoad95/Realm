namespace RealmCore.Server.Modules.World;

public sealed class DirectionalSpawnMarker : SpawnMarker
{
    public Vector3 Position { get; }
    public float Direction { get; }

    public DirectionalSpawnMarker(string name, Vector3 position, float direction) : base(name)
    {
        Position = position;
        Direction = direction;
    }
}

public sealed class PointSpawnMarker : SpawnMarker
{
    public Vector3 Position { get; }

    public PointSpawnMarker(string name, Vector3 position) : base(name)
    {
        Position = position;
    }
}

public abstract class SpawnMarker
{
    protected SpawnMarker(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

public sealed class SpawnMarkersService
{
    private readonly object _lock = new();
    private readonly List<SpawnMarker> _spawnMarkers = [];

    public event Action<SpawnMarker>? SpawnMarkerAdded;

    public IReadOnlyList<SpawnMarker> SpawnMarkers
    {
        get
        {
            lock (_lock)
                return _spawnMarkers.AsReadOnly();
        }
    }

    public void AddSpawnMarker(SpawnMarker spawnMarker)
    {
        lock (_lock)
            _spawnMarkers.Add(spawnMarker);
        SpawnMarkerAdded?.Invoke(spawnMarker);
    }
}

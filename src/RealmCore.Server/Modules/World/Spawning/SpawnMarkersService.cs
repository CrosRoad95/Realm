namespace RealmCore.Server.Modules.World.Spawning;

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

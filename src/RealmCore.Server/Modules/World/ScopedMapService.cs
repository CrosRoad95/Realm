namespace RealmCore.Server.Modules.World;

internal sealed class ScopedMapService : IScopedMapsService
{
    private readonly object _lock = new();
    private readonly ILogger<MapsService> _logger;
    private readonly MapsRegistry _mapsRegistry;
    private readonly RealmPlayer _player;
    private readonly List<string> _loadedMaps = [];

    public IReadOnlyList<string> LoadedMaps
    {
        get
        {
            lock (_lock)
                return [.. _loadedMaps];
        }
    }

    public ScopedMapService(ILogger<MapsService> logger, MapsRegistry mapsRegistry, PlayerContext playerContext)
    {
        _logger = logger;
        _mapsRegistry = mapsRegistry;
        _player = playerContext.Player;
    }

    public bool IsLoaded(string name)
    {
        lock (_lock)
        {
            if (_loadedMaps.Contains(name))
                return false;
        }
        return false;
    }

    public bool Load(string name)
    {
        var map = _mapsRegistry.GetByName(name);
        lock (_lock)
        {
            if (_loadedMaps.Contains(name))
                return false;
            _loadedMaps.Add(name);
            map.LoadFor(_player);
            return true;
        }
    }

    public bool Unload(string name)
    {
        var map = _mapsRegistry.GetByName(name);
        lock (_lock)
        {
            if (!_loadedMaps.Contains(name))
                return false;
            _loadedMaps.Remove(name);
            map.UnloadFor(_player);
            return true;
        }
    }
}

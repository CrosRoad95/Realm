namespace RealmCore.Server.Modules.World;

public enum MapFormat
{
    Xml,
}

public class MapsCollection
{
    private readonly MapIdGenerator _mapIdGenerator;
    private readonly MtaServer _mtaServer;
    private readonly GameWorld _gameWorld;
    private readonly List<MapsDirectoryWatcher> _mapsDirectoryWatchers = [];
    private readonly Dictionary<string, MapBase> _maps = [];
    private readonly object _lock = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MapsCollection> _logger;

    public event Action<string, MapEventType>? MapChanged;

    public IReadOnlyList<string> Maps
    {
        get
        {
            lock (_lock)
                return [.. _maps.Keys];
        }
    }

    public event Action<string, MapBase>? MapAdded;
    public event Action<string, MapBase>? MapRemoved;

    public MapsCollection(IServiceProvider serviceProvider, ILogger<MapsCollection> logger, MapIdGenerator mapIdGenerator, MtaServer mtaServer, GameWorld gameWorld)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _mapIdGenerator = mapIdGenerator;
        _mtaServer = mtaServer;
        _gameWorld = gameWorld;
    }

    public void AddMap(string name, MapBase map)
    {
        lock (_lock)
        {
            if (_maps.ContainsKey(name))
            {
                throw new Exception($"Map of name '{name}' already exists");
            }
            _maps.Add(name, map);
        }

        MapAdded?.Invoke(name, map);
    }

    public void RemoveMapByName(string name)
    {
        MapBase? map;
        lock (_lock)
        {
            if (_maps.TryGetValue(name, out map))
            {
                map.Dispose();
                _maps.Remove(name);
            }
            else
                throw new Exception($"Map of name '{name}' doesn't exists");
        }

        MapRemoved?.Invoke(name, map);
    }

    internal void UnregisterWatcher(MapsDirectoryWatcher mapsDirectoryWatcher)
    {
        if (_mapsDirectoryWatchers.Remove(mapsDirectoryWatcher))
        {
            mapsDirectoryWatcher.MapChanged -= HandleMapChanged;
            mapsDirectoryWatcher.Dispose();
        }
    }

    public MapsWatcherRegistration RegisterMapsPath(string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException(path);

        var mapsDirectoryWatcher = ActivatorUtilities.CreateInstance<MapsDirectoryWatcher>(_serviceProvider, path, this);
        mapsDirectoryWatcher.MapChanged += HandleMapChanged;
        _mapsDirectoryWatchers.Add(mapsDirectoryWatcher);
        return new MapsWatcherRegistration(mapsDirectoryWatcher, this);
    }

    private void HandleMapChanged(string arg1, MapEventType arg2)
    {
        MapChanged?.Invoke(arg1, arg2);
    }

    public MapBase GetByName(string name)
    {
        lock (_lock)
        {
            if (_maps.TryGetValue(name, out var map))
            {
                return map;
            }
            throw new MapNotFoundException(name);
        }
    }
}

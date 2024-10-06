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

    public MapsCollection(IServiceProvider serviceProvider, MapIdGenerator mapIdGenerator, MtaServer mtaServer, GameWorld gameWorld)
    {
        _serviceProvider = serviceProvider;
        _mapIdGenerator = mapIdGenerator;
        _mtaServer = mtaServer;
        _gameWorld = gameWorld;
    }

    public bool AddMap(string name, MapBase map)
    {
        lock (_lock)
        {
            if (_maps.ContainsKey(name))
                return false;

            _maps.Add(name, map);
        }

        MapAdded?.Invoke(name, map);
        return true;
    }

    public bool RemoveMapByName(string name)
    {
        MapBase? map;
        lock (_lock)
        {
            if (!_maps.TryGetValue(name, out map))
                return false;

            map.Dispose();
            _maps.Remove(name);
        }

        MapRemoved?.Invoke(name, map);
        return true;
    }

    internal bool UnregisterWatcher(MapsDirectoryWatcher mapsDirectoryWatcher)
    {
        if (_mapsDirectoryWatchers.Remove(mapsDirectoryWatcher))
        {
            mapsDirectoryWatcher.MapChanged -= HandleMapChanged;
            mapsDirectoryWatcher.Dispose();
            return true;
        }
        return false;
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

    public MapBase? GetByName(string name)
    {
        lock (_lock)
        {
            if (_maps.TryGetValue(name, out var map))
                return map;
        }
        return null;
    }
}

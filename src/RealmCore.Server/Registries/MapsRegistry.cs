namespace RealmCore.Server.Registries;

public class MapsRegistry
{
    private readonly MapIdGenerator _mapIdGenerator;
    private readonly List<MapsDirectoryWatcher> _mapsDirectoryWatchers = [];
    private readonly Dictionary<string, Map> _maps = [];
    private readonly object _lock = new();
    private readonly ILogger<MapsRegistry> _logger;

    public event Action<string, MapEventType>? MapChanged;

    public IReadOnlyList<string> Maps
    {
        get
        {
            lock (_lock)
                return [.. _maps.Keys];
        }
    }
    public event Action<string, Map>? MapAdded;
    public event Action<string, Map>? MapRemoved;

    public MapsRegistry(ILogger<MapsRegistry> logger, MapIdGenerator mapIdGenerator)
    {
        _logger = logger;
        _mapIdGenerator = mapIdGenerator;
    }

    private void AddMap(string name, Map map)
    {
        lock (_lock)
        {
            if (_maps.ContainsKey(name))
            {
                throw new Exception($"Map of name '{name}' already exists");
            }
            _maps.Add(name, map);
            MapAdded?.Invoke(name, map);
        }
    }

    public void RemoveMapByName(string name)
    {
        lock (_lock)
        {
            if (_maps.TryGetValue(name, out var map))
            {
                MapRemoved?.Invoke(name, map);
                map.Dispose();
                _maps.Remove(name);
            }
            else
                throw new Exception($"Map of name '{name}' doesn't exists");
        }
    }

    internal void UnregisterWatcher(MapsDirectoryWatcher mapsDirectoryWatcher)
    {
        if (_mapsDirectoryWatchers.Remove(mapsDirectoryWatcher))
        {
            mapsDirectoryWatcher.MapChanged -= HandleMapChanged;
            mapsDirectoryWatcher.Dispose();
        }
    }

    public MapsWatcherRegistration RegisterMapsPath(string path, IMapsService mapsService)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException(path);

        var mapsDirectoryWatcher = new MapsDirectoryWatcher(path, this, mapsService);
        mapsDirectoryWatcher.MapChanged += HandleMapChanged;
        _mapsDirectoryWatchers.Add(mapsDirectoryWatcher);
        return new MapsWatcherRegistration(mapsDirectoryWatcher, this);
    }

    private void HandleMapChanged(string arg1, MapEventType arg2)
    {
        MapChanged?.Invoke(arg1, arg2);
    }

    public Map GetByName(string name)
    {
        lock (_lock)
        {
            if(_maps.TryGetValue(name, out var map))
            {
                return map;
            }
            throw new MapNotFoundException(name);
        }
    }

    public Map RegisterMapFromMapFile(string name, string fileName)
    {
        lock (_lock)
        {
            if (_maps.ContainsKey(name))
                throw new InvalidOperationException($"Map of name {name} already exists");
        }

        XmlSerializer serializer = new(typeof(XmlMap), "");

        XmlMap? xmlMap = null;
        {
            using var fileStream = File.OpenRead(fileName);
            using var reader = new NamespaceIgnorantXmlTextReader(fileStream);
            xmlMap = (XmlMap?)serializer.Deserialize(reader);
        }

        if (xmlMap == null || xmlMap.Objects.Count == 0)
            throw new InvalidOperationException("Map contains no objects");

        var map = new Map(_mapIdGenerator, xmlMap.Objects.Select(x => new WorldObject((ObjectModel)x.Model, new Vector3(x.PosX, x.PosY, x.PosZ))
        {
            Rotation = new Vector3(x.RotX, x.RotY, x.RotZ),
            Interior = x.Interior,
            Dimension = x.Dimension
        }), name);
        AddMap(name, map);
        return map;
    }

    public void RegisterMapFromMemory(string name, IEnumerable<WorldObject> worldObjects)
    {
        if (!worldObjects.Any())
            throw new InvalidOperationException("Map contains no objects");

        AddMap(name, new Map(_mapIdGenerator, worldObjects, name));
    }
}

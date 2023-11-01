namespace RealmCore.Server.Services;

internal sealed class MapsService : IMapsService
{
    private readonly MapIdGenerator _mapIdGenerator;
    private readonly Dictionary<string, Map> _maps = new();
    private readonly object _lock = new();
    private readonly List<MapsDirectoryWatcher> _mapsDirectoryWatchers = new();
    private readonly ILogger<MapsService> _logger;
    private readonly IElementCollection _elementCollection;

    public List<string> Maps
    {
        get
        {
            lock (_lock)
                return new List<string>(_maps.Keys);
        }
    }

    public MapsService(ILogger<MapsService> logger, IElementCollection elementCollection)
    {
        _mapIdGenerator = new(IdGeneratorConstants.WorldMapIdStart, IdGeneratorConstants.WorldMapIdStop);
        _logger = logger;
        _elementCollection = elementCollection;
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
        }
        _logger.LogInformation("Map {mapName} added", name);
    }
    
    public void RemoveMapByName(string name)
    {
        lock (_lock)
        {
            if(_maps.TryGetValue(name, out var map))
            {
                map.Dispose();
                _maps.Remove(name);
            }
            else
                throw new Exception($"Map of name '{name}' doesn't exists");
        }
        _logger.LogInformation("Map {mapName} removed", name);
    }

    public void LoadMapFor(string name, RealmPlayer player)
    {
        lock (_lock)
        {
            if (!_maps.TryGetValue(name, out var map))
            {
                throw new Exception($"Map of name '{name}' doesn't exists");
            }
            map.LoadFor(player);
        }
    }
    
    public void LoadAllMapsFor(RealmPlayer player)
    {
        lock (_lock)
        {
            foreach (var map in _maps)
                map.Value.LoadFor(player);
        }
    }

    internal void UnregisterWatcher(MapsDirectoryWatcher mapsDirectoryWatcher)
    {
        if(_mapsDirectoryWatchers.Remove(mapsDirectoryWatcher))
           mapsDirectoryWatcher.Dispose();
    }

    public MapsWatcherRegistration RegisterMapsPath(string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException(path);

        var mapsDirectoryWatcher = new MapsDirectoryWatcher(path, this);
        _mapsDirectoryWatchers.Add(mapsDirectoryWatcher);
        return new MapsWatcherRegistration(mapsDirectoryWatcher, this);
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
        }));
        AddMap(name, map);
        return map;
    }

    public void RegisterMapFromMemory(string name, IEnumerable<WorldObject> worldObjects)
    {
        if (!worldObjects.Any())
            throw new InvalidOperationException("Map contains no objects");

        AddMap(name, new Map(_mapIdGenerator, worldObjects));
    }

    public void LoadMapForAll(Map map)
    {
        foreach (var player in _elementCollection.GetByType<Player>())
        {
            map.LoadFor((RealmPlayer)player);
        }
    }
}

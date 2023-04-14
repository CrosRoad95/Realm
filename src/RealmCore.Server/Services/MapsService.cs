namespace RealmCore.Server.Services;

internal class MapsService : IMapsService
{
    private readonly MapIdGenerator _mapIdGenerator;
    private readonly Dictionary<string, Map> _maps = new();
    private readonly object _lock = new();

    public List<string> Maps
    {
        get
        {
            lock (_lock)
                return new List<string>(_maps.Keys);
        }
    }

    public MapsService()
    {
        _mapIdGenerator = new(IdGeneratorConstants.WorldMapIdStart, IdGeneratorConstants.WorldMapIdStop);
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
    }

    public void LoadMapFor(string name, Entity entity)
    {
        var player = entity.Player;
        lock (_lock)
        {
            if (!_maps.TryGetValue(name, out var map))
            {
                throw new Exception($"Map of name '{name}' doesn't exists");
            }
            map.LoadForPlayer(player);
        }
    }

    public void RegisterMapFromXml(string name, string fileName)
    {
        XmlSerializer serializer = new(typeof(XmlMap), "");

        XmlMap xmlMap;
        {
            using var fileStream = File.OpenRead(fileName);
            using var reader = new NamespaceIgnorantXmlTextReader(fileStream);
            xmlMap = (XmlMap)serializer.Deserialize(reader);
        }

        if (!xmlMap.Objects.Any())
            throw new InvalidOperationException("Map contains no objects");

        AddMap(name, new Map(_mapIdGenerator, xmlMap.Objects.Select(x => new WorldObject((ObjectModel)x.Model, new Vector3(x.PosX, x.PosY, x.PosZ))
        {
            Rotation = new Vector3(x.RotX, x.RotY, x.RotZ),
            Interior = x.Interior,
            Dimension = x.Dimension
        })));

    }

    public void RegisterMapFromMemory(string name, IEnumerable<WorldObject> worldObjects)
    {
        if (!worldObjects.Any())
            throw new InvalidOperationException("Map contains no objects");

        AddMap(name, new Map(_mapIdGenerator, worldObjects));
    }
}

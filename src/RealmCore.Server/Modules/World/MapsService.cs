namespace RealmCore.Server.Modules.World;

public sealed class MapsService
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly object _lock = new();
    private readonly ILogger<MapsService> _logger;
    private readonly IElementCollection _elementCollection;
    private readonly MapsCollection _mapsCollection;
    private readonly MapsRepository _mapsRepository;
    private readonly List<string> _loadedMaps = [];
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;

    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.None,
    };

    public IReadOnlyList<string> LoadedMaps
    {
        get
        {
            lock (_lock)
                return [.. _loadedMaps];
        }
    }

    public MapsService(ILogger<MapsService> logger, IElementCollection elementCollection, MapsCollection mapsCollection, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _elementCollection = elementCollection;
        _mapsCollection = mapsCollection;
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _mapsRepository = _serviceProvider.GetRequiredService<MapsRepository>();
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
        var map = _mapsCollection.GetByName(name);
        if (map == null)
            return false;

        lock (_lock)
        {
            if (_loadedMaps.Contains(name))
                return false;
            _loadedMaps.Add(name);
            map.Load();
            return true;
        }
    }

    public bool LoadFor(string name)
    {
        var map = _mapsCollection.GetByName(name);
        if (map == null)
            return false;

        lock (_lock)
        {
            if (_loadedMaps.Contains(name))
                return false;
            _loadedMaps.Add(name);
            foreach (var player in _elementCollection.GetByType<RealmPlayer>())
            {
                map.LoadFor(player);
            }
            return true;
        }
    }

    public bool Unload(string name)
    {
        var map = _mapsCollection.GetByName(name);
        if (map == null)
            return false;

        lock (_lock)
        {
            if (!_loadedMaps.Contains(name))
                return false;
            _loadedMaps.Remove(name);
            map.Unload();
            return true;
        }
    }

    public async Task<bool> CreatePersistentMapFromFileUploadForUser(int fileUploadId, int userId, int locationId, object? metadata, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var metadataString = Serialize(metadata);
            return await _mapsRepository.CreatePersistentMapFromFileUploadForUser(fileUploadId, userId, locationId, metadataString, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<bool> LoadPersistentMap(int mapId, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _mapsRepository.SetMapLoaded(mapId, true, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<bool> UnloadPersistentMap(int mapId, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _mapsRepository.SetMapLoaded(mapId, false, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private string Serialize(object? data) => JsonConvert.SerializeObject(data, _jsonSerializerSettings);
}

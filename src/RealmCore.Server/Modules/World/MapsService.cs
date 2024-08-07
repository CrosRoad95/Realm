﻿namespace RealmCore.Server.Modules.World;

public sealed class MapsService
{
    private readonly object _lock = new();
    private readonly ILogger<MapsService> _logger;
    private readonly IElementCollection _elementCollection;
    private readonly MapsCollection _mapsCollection;
    private readonly List<string> _loadedMaps = [];

    public IReadOnlyList<string> LoadedMaps
    {
        get
        {
            lock (_lock)
                return [.. _loadedMaps];
        }
    }

    public MapsService(ILogger<MapsService> logger, IElementCollection elementCollection, MapsCollection mapsCollection)
    {
        _logger = logger;
        _elementCollection = elementCollection;
        _mapsCollection = mapsCollection;
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
        lock (_lock)
        {
            if (!_loadedMaps.Contains(name))
                return false;
            _loadedMaps.Remove(name);
            map.Unload();
            return true;
        }
    }
}

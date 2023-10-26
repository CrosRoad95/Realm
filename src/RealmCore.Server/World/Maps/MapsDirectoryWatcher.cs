using RealmCore.Server.Utilities;

namespace RealmCore.Server.World.Maps;

internal class MapsDirectoryWatcher : IDisposable
{
    private enum MapEventType
    {
        Add, Remove, Update
    }

    private struct MapEvent
    {
        public MapEventType mapEventType;
        public string mapName;
    }

    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly string _path;
    private readonly MapsService _mapsService;
    private readonly List<MapEvent> _mapEvents = new();
    private readonly object _mapEventsLock = new();
    private readonly Debounce _debounce = new(250);

    public MapsDirectoryWatcher(string path, MapsService mapsService)
    {
        _path = path;
        _mapsService = mapsService;
        string searchPattern = "*.map";
        _fileSystemWatcher = new FileSystemWatcher
        {
            Path = path,
            IncludeSubdirectories = true
        };
        _fileSystemWatcher.Created += HandleCreated;
        _fileSystemWatcher.Changed += HandleChanged;
        _fileSystemWatcher.Deleted += HandleDeleted;
        _fileSystemWatcher.Filters.Add(searchPattern);
        _fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName |
                       NotifyFilters.LastAccess |
                       NotifyFilters.LastWrite |
                       NotifyFilters.FileName |
                       NotifyFilters.Size |
                       NotifyFilters.Attributes;
        _fileSystemWatcher.EnableRaisingEvents = true;

        string[] files = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        foreach (var mapPath in files)
        {
            HandleMapCreated(mapPath);
        }
    }

    private async Task ScheduleFlush()
    {
        try
        {
            await _debounce.InvokeAsync(Flush);
        }
        catch (Exception ex)
        {
            // Ignore
        }
    }

    private string GetMapName(string mapFile)
    {
        string mapName;
        if (string.IsNullOrEmpty(Path.GetDirectoryName(mapFile)))
        {
            mapName = Path.GetFileNameWithoutExtension(mapFile);
        }
        else
        {
            mapName = (Path.GetDirectoryName(mapFile) + "/" + Path.GetFileNameWithoutExtension(mapFile));
        }
        return mapName.Replace("\\", "/");
    }

    private void Flush()
    {
        IReadOnlyList<MapEvent> mapEvents;
        lock (_mapEventsLock)
        {
            mapEvents = new List<MapEvent>(_mapEvents.DistinctBy(x => x.mapName));
            _mapEvents.Clear();
        }

        foreach (var mapFile in mapEvents.Where(x => x.mapEventType == MapEventType.Remove).Select(x => x.mapName))
        {
            var mapName = GetMapName(mapFile);

            _mapsService.RemoveMapByName(mapName);
        }

        foreach (var mapFile in mapEvents.Where(x => x.mapEventType == MapEventType.Add).Select(x => x.mapName))
        {
            var mapName = GetMapName(mapFile);

            var map = _mapsService.RegisterMapFromMapFile(mapName, Path.Combine(_path, mapFile));
            _mapsService.LoadMapForAll(map);
        }

        foreach (var mapFile in mapEvents.Where(x => x.mapEventType == MapEventType.Update).Select(x => x.mapName))
        {
            var mapName = GetMapName(mapFile);

            _mapsService.RemoveMapByName(mapName);
            var map = _mapsService.RegisterMapFromMapFile(mapName, Path.Combine(_path, mapFile));
            _mapsService.LoadMapForAll(map);
        }
    }

    private void HandleCreated(object sender, FileSystemEventArgs e)
    {
        HandleMapCreated(e.FullPath);
    }

    private void HandleChanged(object sender, FileSystemEventArgs e)
    {
        lock (_mapEventsLock)
        {
            _mapEvents.Add(new MapEvent
            {
                mapEventType = MapEventType.Update,
                mapName = Path.GetRelativePath(_path, e.FullPath),
            });
        }
        Task.Run(ScheduleFlush);
    }

    private void HandleDeleted(object sender, FileSystemEventArgs e)
    {
        lock (_mapEventsLock)
        {
            _mapEvents.Add(new MapEvent
            {
                mapEventType = MapEventType.Remove,
                mapName = Path.GetRelativePath(_path, e.FullPath),
            });
        }
        Task.Run(ScheduleFlush);
    }

    private void HandleMapCreated(string path)
    {
        lock (_mapEventsLock)
        {
            _mapEvents.Add(new MapEvent
            {
                mapEventType = MapEventType.Add,
                mapName = Path.GetRelativePath(_path, path),
            });
        }
        Task.Run(ScheduleFlush);
    }

    public void Dispose()
    {
        _fileSystemWatcher.Dispose();
    }

}

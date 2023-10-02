namespace RealmCore.Server.World.Maps;

public struct MapsWatcherRegistration
{
    private readonly MapsDirectoryWatcher _mapsDirectoryWatcher;
    private readonly MapsService _mapsService;

    internal MapsWatcherRegistration(MapsDirectoryWatcher mapsDirectoryWatcher, MapsService mapsService)
    {
        _mapsDirectoryWatcher = mapsDirectoryWatcher;
        _mapsService = mapsService;
    }

    public void Unregister()
    {
        _mapsService.UnregisterWatcher(_mapsDirectoryWatcher);
    }
}

namespace RealmCore.Server.World.Maps;

public struct MapsWatcherRegistration
{
    private readonly MapsDirectoryWatcher _mapsDirectoryWatcher;
    private readonly MapsRegistry _mapsRegistry;

    internal MapsWatcherRegistration(MapsDirectoryWatcher mapsDirectoryWatcher, MapsRegistry mapsRegistry)
    {
        _mapsDirectoryWatcher = mapsDirectoryWatcher;
        _mapsRegistry = mapsRegistry;
    }

    public void Unregister()
    {
        _mapsRegistry.UnregisterWatcher(_mapsDirectoryWatcher);
    }
}

namespace RealmCore.Server.Modules.World;

public struct MapsWatcherRegistration
{
    private readonly MapsDirectoryWatcher _mapsDirectoryWatcher;
    private readonly MapsCollection _mapsCollection;

    internal MapsWatcherRegistration(MapsDirectoryWatcher mapsDirectoryWatcher, MapsCollection mapsCollection)
    {
        _mapsDirectoryWatcher = mapsDirectoryWatcher;
        _mapsCollection = mapsCollection;
    }

    public void Unregister()
    {
        _mapsCollection.UnregisterWatcher(_mapsDirectoryWatcher);
    }
}

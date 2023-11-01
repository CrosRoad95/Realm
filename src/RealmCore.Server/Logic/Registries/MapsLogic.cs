namespace RealmCore.Server.Logic.Registries;

internal sealed class MapsLogic
{
    private readonly IMapsService _mapsService;
    private readonly MapsRegistry _mapsRegistry;

    public MapsLogic(IMapsService mapsService, RealmServer realmServer, MapsRegistry mapsRegistry)
    {
        _mapsService = mapsService;
        _mapsRegistry = mapsRegistry;
        realmServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(RealmPlayer player)
    {
        foreach (var map in _mapsService.LoadedMaps)
        {
            _mapsRegistry.GetByName(map).LoadFor(player);
        }
    }
}

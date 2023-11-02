namespace RealmCore.Server.Logic.Registries;

internal sealed class MapsLogic
{
    private readonly IMapsService _mapsService;
    private readonly MapsRegistry _mapsRegistry;

    public MapsLogic(IMapsService mapsService, MtaServer mtaServer, MapsRegistry mapsRegistry)
    {
        _mapsService = mapsService;
        _mapsRegistry = mapsRegistry;
        mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        foreach (var map in _mapsService.LoadedMaps)
        {
            _mapsRegistry.GetByName(map).LoadFor((RealmPlayer)player);
        }
    }
}

namespace RealmCore.Server.Modules.World;

internal sealed class MapsLogic
{
    private readonly IMapsService _mapsService;
    private readonly MapsCollection _mapsCollection;

    public MapsLogic(IMapsService mapsService, MtaServer mtaServer, MapsCollection mapsCollection)
    {
        _mapsService = mapsService;
        _mapsCollection = mapsCollection;
        mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        foreach (var map in _mapsService.LoadedMaps)
        {
            _mapsCollection.GetByName(map).LoadFor((RealmPlayer)player);
        }
    }
}

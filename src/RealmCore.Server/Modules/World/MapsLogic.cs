namespace RealmCore.Server.Modules.World;

internal sealed class MapsLogic
{
    private readonly IMapsService _mapsService;
    private readonly MapsCollection _mapsCollection;

    public MapsLogic(IMapsService mapsService, MapsCollection mapsCollection, IPlayersEventManager playersEventManager)
    {
        _mapsService = mapsService;
        _mapsCollection = mapsCollection;
        playersEventManager.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        foreach (var map in _mapsService.LoadedMaps)
        {
            _mapsCollection.GetByName(map).LoadFor((RealmPlayer)player);
        }
    }
}

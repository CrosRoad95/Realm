namespace RealmCore.Server.Modules.World;

internal sealed class MapsLogic : PlayerLifecycle
{
    private readonly IMapsService _mapsService;
    private readonly MapsCollection _mapsCollection;

    public MapsLogic(IMapsService mapsService, MapsCollection mapsCollection, PlayersEventManager playersEventManager) : base(playersEventManager)
    {
        _mapsService = mapsService;
        _mapsCollection = mapsCollection;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        foreach (var map in _mapsService.LoadedMaps)
        {
            _mapsCollection.GetByName(map).LoadFor((RealmPlayer)player);
        }
    }
}

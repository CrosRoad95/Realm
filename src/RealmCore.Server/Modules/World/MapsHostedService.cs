namespace RealmCore.Server.Modules.World;

internal sealed class MapsHostedService : PlayerLifecycle, IHostedService
{
    private readonly IMapsService _mapsService;
    private readonly MapsCollection _mapsCollection;

    public MapsHostedService(IMapsService mapsService, MapsCollection mapsCollection, PlayersEventManager playersEventManager) : base(playersEventManager)
    {
        _mapsService = mapsService;
        _mapsCollection = mapsCollection;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        foreach (var map in _mapsService.LoadedMaps)
        {
            _mapsCollection.GetByName(map).LoadFor(player);
        }
    }
}

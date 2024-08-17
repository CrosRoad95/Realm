using RealmCore.Resources.Browser;

namespace RealmCore.BlazorGui.Logic;

internal sealed class AssetsManager : BackgroundService
{
    private readonly AssetsCollection _assetsCollection;
    private readonly IAssetsService _assetsService;
    private readonly IBrowserService _browserService;

    public AssetsManager(AssetsCollection assetsCollection, IAssetsService assetsService, IBrowserService browserService)
    {
        _assetsCollection = assetsCollection;
        _assetsService = assetsService;
        _browserService = browserService;
        _assetsService.ReplaceModel((ObjectModel)1337, "ConeModel", "ModelsTextures");
        //_assetsService.ReplaceModel((ObjectModel)1337, "ConeModel", "ConeCollision", "ModelsTextures");
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _assetsCollection.AddRemoteImage(_browserService.HttpClient, "assets/map.jpg");
    }
}

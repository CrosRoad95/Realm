namespace RealmCore.BlazorGui.Logic;

internal sealed class AssetsManager : BackgroundService
{
    private readonly AssetsCollection _assetsCollection;
    private readonly IAssetsService _assetsService;

    public AssetsManager(AssetsCollection assetsCollection, IAssetsService assetsService)
    {
        _assetsCollection = assetsCollection;
        _assetsService = assetsService;

        _assetsService.ReplaceModel((ObjectModel)1337, "ConeModel", "ModelsTextures");
        //_assetsService.ReplaceModel((ObjectModel)1337, "ConeModel", "ConeCollision", "ModelsTextures");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {

        return Task.CompletedTask;
    }
}

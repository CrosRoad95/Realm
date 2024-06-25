namespace RealmCore.BlazorGui.Logic;

internal sealed class AssetsManager : BackgroundService
{
    private const string _basePath = "../../../Server/Assets";
    private readonly AssetsCollection _assetsCollection;

    public AssetsManager(AssetsCollection assetsCollection)
    {
        _assetsCollection = assetsCollection;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {

        return Task.CompletedTask;
    }
}

using RealmCore.Resources.Assets.Factories;

namespace RealmCore.BlazorGui.Logic;

internal sealed class ProceduralObjectsHostedService : IHostedLifecycleService
{
    private readonly AssetsCollection _assetsCollection;
    private readonly IAssetsService _assetsService;

    public ProceduralObjectsHostedService(AssetsCollection assetsCollection, IAssetsService assetsService)
    {
        _assetsCollection = assetsCollection;
        _assetsService = assetsService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        //var modelFactory = new ModelFactory();
        //modelFactory.AddTriangle(new Vector3(2, 2, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0), "brickred");
        //modelFactory.AddTriangle(new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(1, 10, 0), "brickred");
        //var dffStream = modelFactory.BuildDFF();
        //var colStream = modelFactory.BuildCOL();

        //var dff = _assetsCollection.AddDFF("testDFF", dffStream);
        //var col = _assetsCollection.AddCOL("testCOL", colStream);
        //_assetsService.ReplaceModel((ObjectModel)1338, "testDFF", "testCOL", "ModelsTextures");

        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

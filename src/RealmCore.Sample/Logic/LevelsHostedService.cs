
namespace RealmCore.Sample.Logic;

internal class LevelsHostedService : IHostedService
{
    public LevelsHostedService(LevelsCollection levelsCollection)
    {
        for (uint i = 1; i < 100; i++)
        {
            levelsCollection.Add(i, new LevelsCollectionItem(i * 25));
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

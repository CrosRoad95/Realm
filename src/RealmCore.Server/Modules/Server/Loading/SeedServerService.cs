
namespace RealmCore.Server.Modules.Server.Loading;

internal sealed class SeedServerService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public SeedServerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        using var seederServerBuilder = scope.ServiceProvider.GetRequiredService<SeederServerBuilder>();
        await seederServerBuilder.Build(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

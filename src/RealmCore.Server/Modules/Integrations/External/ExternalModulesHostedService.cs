
namespace RealmCore.Server.Modules.Integrations.External;

public sealed class ExternalModulesHostedService : IHostedService
{
    private readonly List<IExternalModule> _modules;

    public ExternalModulesHostedService(IEnumerable<IExternalModule> modules)
    {
        _modules = modules.ToList();
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

using RealmCore.Server.Extensions;
using RealmCore.Server.Interfaces;

namespace RealmCore.BlazorHelpers;

internal class RealmServerHostedService : IHostedService
{
    private readonly IRealmServer _realmServer;
    private readonly ILogger<RealmServerHostedService> _logger;

    public RealmServerHostedService(IRealmServer realmServer, ILogger<RealmServerHostedService> logger)
    {
        _realmServer = realmServer;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _realmServer.Start();
        }
        catch(Exception ex)
        {
            _logger.LogHandleError(ex);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

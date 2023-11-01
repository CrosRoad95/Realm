using RealmCore.Server;
using RealmCore.Server.Extensions;

namespace RealmCore.BlazorHelpers;

internal class RealmServerHostedService : IHostedService
{
    private readonly RealmServer _realmServer;
    private readonly ILogger<RealmServerHostedService> _logger;

    public RealmServerHostedService(RealmServer realmServer, ILogger<RealmServerHostedService> logger)
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

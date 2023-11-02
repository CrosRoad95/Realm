using RealmCore.Server.Extensions;
using SlipeServer.Server;

namespace RealmCore.BlazorHelpers;

internal class RealmServerHostedService : IHostedService
{
    private readonly MtaServer _realmServer;
    private readonly ILogger<RealmServerHostedService> _logger;

    public RealmServerHostedService(MtaServer mtaServer, ILogger<RealmServerHostedService> logger)
    {
        _realmServer = mtaServer;
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

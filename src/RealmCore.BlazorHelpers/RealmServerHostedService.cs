using RealmCore.Server.Extensions;

namespace RealmCore.BlazorHelpers;

internal class RealmServerHostedService : IHostedService
{
    private readonly MtaServer _mtaServer;
    private readonly ILogger<RealmServerHostedService> _logger;

    public RealmServerHostedService(MtaServer mtaServer, ILogger<RealmServerHostedService> logger)
    {
        _mtaServer = mtaServer;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _mtaServer.Start();
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

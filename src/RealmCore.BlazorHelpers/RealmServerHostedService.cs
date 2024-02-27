using RealmCore.Server.Extensions;

namespace RealmCore.BlazorHelpers;

internal class RealmServerHostedService : IHostedService
{
    private readonly MtaServer _server;
    private readonly ILogger<RealmServerHostedService> _logger;

    public RealmServerHostedService(MtaServer server, ILogger<RealmServerHostedService> logger)
    {
        _server = server;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _server.Start();
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

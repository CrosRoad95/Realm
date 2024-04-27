using Microsoft.Extensions.Hosting;

namespace RealmCore.Module.Grpc;

internal sealed class GrpcServerHostedService : IHostedService
{
    private readonly Server _server;

    public GrpcServerHostedService(Server server)
    {
        _server = server;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
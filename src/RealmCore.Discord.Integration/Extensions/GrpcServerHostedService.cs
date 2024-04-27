using Microsoft.Extensions.Hosting;
using RealmCore.Module.Grpc;

namespace RealmCore.Discord.Integration.Extensions;

internal sealed class GrpcServerHostedService : IHostedService
{
    private readonly IGrpcServer _grpcServer;

    public GrpcServerHostedService(IGrpcServer grpcServer)
    {
        _grpcServer = grpcServer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _grpcServer.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

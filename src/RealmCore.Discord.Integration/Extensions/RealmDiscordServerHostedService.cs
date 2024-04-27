using Microsoft.Extensions.Hosting;

namespace RealmCore.Discord.Integration.Extensions;

internal sealed class RealmDiscordServerHostedService : IHostedService
{
    private readonly GrpcChannel _grpcChannel;

    public RealmDiscordServerHostedService(GrpcChannel grpcChannel)
    {
        _grpcChannel = grpcChannel;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _grpcChannel.ConnectAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

using Microsoft.Extensions.Hosting;

namespace RealmCore.Discord.Integration.Extensions;

internal sealed class RealmDiscordHostedService : IHostedService
{
    private readonly IRealmDiscordClient _realmDiscordClient;
    private readonly GrpcServer _grpcServer;

    public RealmDiscordHostedService(IRealmDiscordClient realmDiscordClient, GrpcServer grpcServer)
    {
        _realmDiscordClient = realmDiscordClient;
        _grpcServer = grpcServer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _grpcServer.Start();
        await _realmDiscordClient.StartAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

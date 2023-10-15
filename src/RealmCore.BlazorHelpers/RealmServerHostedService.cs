using Microsoft.Extensions.Hosting;
using RealmCore.Server.Interfaces;

namespace RealmCore.BlazorHelpers;

internal class RealmServerHostedService : IHostedService
{
    private readonly IRealmServer _realmServer;

    public RealmServerHostedService(IRealmServer realmServer)
    {
        _realmServer = realmServer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _realmServer.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

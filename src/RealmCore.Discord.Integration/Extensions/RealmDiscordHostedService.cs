using Microsoft.Extensions.Hosting;

namespace RealmCore.Discord.Integration.Extensions;

internal sealed class RealmDiscordHostedService : IHostedService
{
    private readonly IRealmDiscordClient _realmDiscordClient;

    public RealmDiscordHostedService(IRealmDiscordClient realmDiscordClient)
    {
        _realmDiscordClient = realmDiscordClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _realmDiscordClient.StartAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

using Realm.Module.Discord.Interfaces;

namespace Realm.Server.Integrations.Discord.Handlers;

public class DefaultDiscordConnectAccountHandler : IDiscordConnectAccountHandler
{
    private readonly IECS _ecs;
    private readonly ILogger<DefaultDiscordConnectAccountHandler> _logger;

    public DefaultDiscordConnectAccountHandler(IECS ecs, ILogger<DefaultDiscordConnectAccountHandler> logger)
    {
        _ecs = ecs;
        _logger = logger;
    }

    public Task<TryConnectResponse> HandleConnectAccount(string code, ulong userId, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var item in _ecs.PlayerEntities)
            {
                if(item.TryGetComponent<PendingDiscordIntegrationComponent>(out var component))
                {
                    if (component.Verify(code))
                    {
                        item.AddComponent(new DiscordIntegrationComponent(userId));
                        return Task.FromResult(new TryConnectResponse
                        {
                            message = "Account connected successfully!",
                            success = true,
                        });
                    }
                }
            }

            return Task.FromResult(new TryConnectResponse
            {
                message = "Unexpected error while trying to connect account",
                success = false,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while trying to connect account");
            return Task.FromResult(new TryConnectResponse
            {
                message = "Unexpected error while trying to connect account",
                success = false,
            });
            throw;
        }
    }
}

using Realm.Module.Discord.Interfaces;

namespace Realm.Server.Integrations.Discord.Handlers;

public class DefaultDiscordConnectAccountHandler : IDiscordConnectAccountHandler
{
    private readonly ECS _ecs;
    private readonly ILogger<DefaultDiscordConnectAccountHandler> _logger;

    public DefaultDiscordConnectAccountHandler(ECS ecs, ILogger<DefaultDiscordConnectAccountHandler> logger)
    {
        _ecs = ecs;
        _logger = logger;
    }

    public async Task<TryConnectResponse> HandleConnectAccount(string code, ulong userId, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var item in _ecs.GetPlayerEntities())
            {
                if(item.TryGetComponent<PendingDiscordIntegrationComponent>(out var component))
                {
                    if (component.Verify(code))
                    {
                        item.AddComponent(new DiscordIntegrationComponent(userId));
                        return new TryConnectResponse
                        {
                            message = "Account connected successfully!",
                            success = true,
                        };
                    }
                }
            }

            return new TryConnectResponse
            {
                message = "Unexpected error while trying to connect account",
                success = false,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while trying to connect account");
            return new TryConnectResponse
            {
                message = "Unexpected error while trying to connect account",
                success = false,
            };
            throw;
        }
    }
}

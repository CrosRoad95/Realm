using RealmCore.Server.Components.Players;

namespace RealmCore.Console.Integrations.Discord.Handlers;

public class DiscordConnectUserHandler : IDiscordConnectUserHandler
{
    private readonly IECS _ecs;
    private readonly ILogger<DiscordConnectUserHandler> _logger;

    public DiscordConnectUserHandler(IECS ecs, ILogger<DiscordConnectUserHandler> logger)
    {
        _ecs = ecs;
        _logger = logger;
    }

    public Task<TryConnectResponse> HandleConnectUser(string code, ulong userId, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var item in _ecs.PlayerEntities)
            {
                if (item.TryGetComponent<PendingDiscordIntegrationComponent>(out var component))
                {
                    if (component.Verify(code))
                    {
                        item.AddComponent(new DiscordIntegrationComponent(userId));
                        return Task.FromResult(new TryConnectResponse
                        {
                            message = "User connected successfully!",
                            success = true,
                        });
                    }
                }
            }

            return Task.FromResult(new TryConnectResponse
            {
                message = "Unexpected error while trying to connect user",
                success = false,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while trying to connect user");
            return Task.FromResult(new TryConnectResponse
            {
                message = "Unexpected error while trying to connect user",
                success = false,
            });
            throw;
        }
    }
}

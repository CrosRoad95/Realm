using RealmCore.Module.Discord.Interfaces;
using RealmCore.Module.Discord.Services;
using RealmCore.Server.Modules.Elements;

namespace RealmCore.Console.Extra.Integrations.Discord.Handlers;

public class DiscordConnectUserHandler : IDiscordConnectUserHandler
{
    private readonly IElementCollection _elementCollection;
    private readonly ILogger<DiscordConnectUserHandler> _logger;

    public DiscordConnectUserHandler(IElementCollection elementCollection, ILogger<DiscordConnectUserHandler> logger)
    {
        _elementCollection = elementCollection;
        _logger = logger;
    }

    public Task<TryConnectResponse> HandleConnectUser(string code, ulong discordUserId, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var player in _elementCollection.GetByType<RealmPlayer>())
            {
                // TODO:
                //if (player.TryGetComponent<PendingDiscordIntegrationComponent>(out var component))
                //{
                //    if (component.Verify(code))
                //    {
                //        player.AddComponent(new DiscordIntegrationComponent(discordUserId));
                //        return Task.FromResult(new TryConnectResponse
                //        {
                //            message = "User connected successfully!",
                //            success = true,
                //        });
                //    }
                //}
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

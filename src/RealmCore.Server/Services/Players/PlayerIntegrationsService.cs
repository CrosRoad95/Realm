using RealmCore.Server.Services.Integrations;

namespace RealmCore.Server.Services.Players;

public class PlayerIntegrationsService : IPlayerIntegrationsService
{
    public IDiscordIntegration Discord { get; private set; }
    public RealmPlayer Player { get; private set; }
    public PlayerIntegrationsService(PlayerContext playerContext, IDateTimeProvider dateTimeProvider, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        Discord = new DiscordIntegration(Player, dateTimeProvider);
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        if (playerUserService.User.DiscordIntegration != null)
            Discord.DiscordUserId = playerUserService.User.DiscordIntegration.DiscordUserId;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {
        throw new NotImplementedException();
    }

}

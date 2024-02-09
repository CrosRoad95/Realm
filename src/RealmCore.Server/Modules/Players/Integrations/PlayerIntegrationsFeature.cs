namespace RealmCore.Server.Modules.Players.Integrations;

public interface IPlayerIntegrationsFeature : IPlayerFeature
{
    IDiscordIntegration Discord { get; }
}

internal sealed class PlayerIntegrationsFeature : IPlayerIntegrationsFeature
{
    public IDiscordIntegration Discord { get; private set; }
    public RealmPlayer Player { get; init; }
    public PlayerIntegrationsFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider, IPlayerUserFeature playerUserService)
    {
        Player = playerContext.Player;
        Discord = new DiscordIntegration(Player, dateTimeProvider);
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserService, RealmPlayer _)
    {
        if (playerUserService.User.DiscordIntegration != null)
            Discord.DiscordUserId = playerUserService.User.DiscordIntegration.DiscordUserId;
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserService, RealmPlayer _)
    {
    }
}

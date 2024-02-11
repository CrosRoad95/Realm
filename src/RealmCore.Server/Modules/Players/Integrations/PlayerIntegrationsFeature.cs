namespace RealmCore.Server.Modules.Players.Integrations;

public interface IPlayerIntegrationsFeature : IPlayerFeature
{
    /// <summary>
    /// Integration with discord
    /// </summary>
    IDiscordIntegration Discord { get; }
    /// <summary>
    /// Array of all possible integrations
    /// </summary>
    IIntegration[] Integrations { get; }
}

internal sealed class PlayerIntegrationsFeature : IPlayerIntegrationsFeature
{
    public IDiscordIntegration Discord { get; private set; }
    public RealmPlayer Player { get; init; }

    public IIntegration[] Integrations => [Discord];

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
            Discord.Integrate(playerUserService.User.DiscordIntegration.DiscordUserId);
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserService, RealmPlayer _)
    {
        Discord.TryRemove();
    }
}

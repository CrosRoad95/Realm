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

internal sealed class PlayerIntegrationsFeature : IPlayerIntegrationsFeature, IDisposable
{
    public IDiscordIntegration Discord { get; private set; }
    public RealmPlayer Player { get; init; }

    public IIntegration[] Integrations => [Discord];

    public PlayerIntegrationsFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider, IPlayerUserFeature playerUserFeature)
    {
        Player = playerContext.Player;
        Discord = new DiscordIntegration(Player, dateTimeProvider);
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        if (playerUserFeature.UserData.DiscordIntegration != null)
            Discord.Integrate(playerUserFeature.UserData.DiscordIntegration.DiscordUserId);
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        Discord.TryRemove();
    }

    public void Dispose()
    {
        Discord.TryRemove();
    }
}

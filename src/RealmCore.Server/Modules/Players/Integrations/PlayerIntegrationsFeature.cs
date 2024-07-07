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

internal sealed class PlayerIntegrationsFeature : IPlayerIntegrationsFeature, IUsesUserPersistentData
{
    public IDiscordIntegration Discord { get; private set; }
    public RealmPlayer Player { get; init; }

    public IIntegration[] Integrations => [Discord];

    public PlayerIntegrationsFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        Discord = new DiscordIntegration(Player, dateTimeProvider);
    }

    public event Action? VersionIncreased;

    public void LogIn(UserData userData)
    {
        if (userData.DiscordIntegration != null)
            Discord.Integrate(userData.DiscordIntegration.DiscordUserId, userData.DiscordIntegration.Name);
    }
}

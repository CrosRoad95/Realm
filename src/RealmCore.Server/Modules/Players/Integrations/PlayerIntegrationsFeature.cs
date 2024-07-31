namespace RealmCore.Server.Modules.Players.Integrations;

public sealed class PlayerIntegrationsFeature : IPlayerFeature, IUsesUserPersistentData, IDisposable
{
    private UserData? _userData;
    public IDiscordIntegration Discord { get; private set; }
    public RealmPlayer Player { get; init; }

    public IIntegration[] Integrations => [Discord];

    public PlayerIntegrationsFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        Discord = new DiscordIntegration(Player, dateTimeProvider);

        Discord.Created += HandleDiscordCreated;
        Discord.Removed += HandleDiscordRemoved;
    }

    private void HandleDiscordCreated(IIntegration integration)
    {
        if (integration is not IDiscordIntegration discordIntegration)
            return;

        if(_userData != null)
        {
            VersionIncreased?.Invoke();
            _userData.DiscordIntegration = new DiscordIntegrationData
            {
                DiscordUserId = discordIntegration.DiscordUserId,
                Name = discordIntegration.Name
            };
        }
    }

    private void HandleDiscordRemoved(IIntegration _)
    {
        if (_userData != null)
        {
            VersionIncreased?.Invoke();
            _userData.DiscordIntegration = null;
        }
    }

    public event Action? VersionIncreased;

    public void LogIn(UserData userData)
    {
        _userData = userData;
        if (userData.DiscordIntegration != null)
            Discord.Integrate(userData.DiscordIntegration.DiscordUserId, userData.DiscordIntegration.Name ?? "");
    }

    public void Dispose()
    {
        Discord.Created -= HandleDiscordCreated;
        Discord.Removed -= HandleDiscordRemoved;
    }
}

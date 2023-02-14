namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class DiscordIntegrationComponent : IntegrationComponent
{
    private readonly ulong _discordUserId;
    public ulong DiscordUserId => _discordUserId;

    public DiscordIntegrationComponent(ulong discordId)
    {
        _discordUserId = discordId;
    }
}

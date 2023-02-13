namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class DiscordIntegrationComponent : IntegrationComponent
{
    private readonly ulong _discordId;

    public DiscordIntegrationComponent(ulong discordId)
    {
        _discordId = discordId;
    }
}

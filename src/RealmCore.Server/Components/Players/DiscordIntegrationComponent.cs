using RealmCore.Server.Components.Abstractions;

namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class DiscordIntegrationComponent : IntegrationComponent
{
    private readonly ulong _discordUserId;
    public ulong DiscordUserId
    {
        get
        {
            ThrowIfDisposed();
            return _discordUserId;
        }
    }

    public DiscordIntegrationComponent(ulong discordId)
    {
        _discordUserId = discordId;
    }
}

namespace Realm.Module.Discord.Services;

internal class DiscordService : IDiscordService
{
    public DiscordService()
    {
    }

    public UpdateStatusChannel? UpdateStatusChannel { get; set; }
    public TryConnectAccountChannel? TryConnectAccountChannel { get; set; }
}

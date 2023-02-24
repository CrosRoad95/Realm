using Realm.Module.Discord.Interfaces;

namespace Realm.Module.Grpc.Services;

internal class DiscordService : IDiscordService
{
    public DiscordService()
    {
    }

    public UpdateStatusChannel? UpdateStatusChannel { get; set; }
    public TryConnectAccountChannel? TryConnectAccountChannel { get; set; }
}

namespace Realm.GRpc.Services;

internal class DiscordService : IGrpcDiscord
{

    public DiscordService()
    {
    }

    public UpdateStatusChannel? UpdateStatusChannel { get; set; }
}

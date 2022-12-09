namespace Realm.Module.Grpc.Services;

internal class DiscordService : IGrpcDiscord
{

    public DiscordService()
    {
    }

    public UpdateStatusChannel? UpdateStatusChannel { get; set; }
}

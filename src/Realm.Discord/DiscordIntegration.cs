using Realm.Interfaces.Grpc;

namespace Realm.Module.Discord;

internal class DiscordIntegration
{
    public DiscordIntegration(IGrpcDiscord grpcDiscord)
    {
        grpcDiscord.UpdateStatusChannel = HandleUpdateStatusChannel;
    }

    public async Task<string> HandleUpdateStatusChannel()
    {
        return "";
    }
}
using Realm.Interfaces.Grpc;
using Realm.Module.Discord.Interfaces;

namespace Realm.Module.Discord;

internal class DiscordIntegration
{
    private readonly IDiscordStatusChannelUpdateHandler? _discordStatusChannelUpdateHandler;

    public DiscordIntegration(IGrpcDiscord grpcDiscord, IDiscordStatusChannelUpdateHandler? discordStatusChannelUpdateHandler = null)
    {
        grpcDiscord.UpdateStatusChannel = HandleUpdateStatusChannel;
        _discordStatusChannelUpdateHandler = discordStatusChannelUpdateHandler;
    }

    public async Task<string> HandleUpdateStatusChannel()
    {
        if (_discordStatusChannelUpdateHandler == null)
            return "Discord status channel update handler not configured properly!";

        return await _discordStatusChannelUpdateHandler.HandleStatusUpdate();
    }
}
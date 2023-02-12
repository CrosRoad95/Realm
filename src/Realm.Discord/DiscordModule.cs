using Realm.Interfaces.Extend;
using Realm.Interfaces.Grpc;
using Realm.Module.Discord.Interfaces;

namespace Realm.Module.Discord;

internal class DiscordModule : IModule
{
    private readonly IDiscordStatusChannelUpdateHandler? _discordStatusChannelUpdateHandler;

    public DiscordModule(IGrpcDiscord grpcDiscord, IDiscordStatusChannelUpdateHandler? discordStatusChannelUpdateHandler = null)
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
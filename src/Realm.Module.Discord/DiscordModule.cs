using Realm.Interfaces.Extend;
using Realm.Module.Discord.Interfaces;

namespace Realm.Module.Discord;

internal class DiscordModule : IModule
{
    private readonly IDiscordStatusChannelUpdateHandler? _discordStatusChannelUpdateHandler;
    private readonly IDiscordConnectAccountHandler? _discordConnectAccountHandler;

    public DiscordModule(IDiscordService grpcDiscord,
        IDiscordStatusChannelUpdateHandler? discordStatusChannelUpdateHandler = null,
        IDiscordConnectAccountHandler? discordConnectAccountHandler = null)
    {
        grpcDiscord.UpdateStatusChannel = HandleUpdateStatusChannel;
        grpcDiscord.TryConnectAccountChannel = HandleTryConnectAccountChannel;
        _discordStatusChannelUpdateHandler = discordStatusChannelUpdateHandler;
        _discordConnectAccountHandler = discordConnectAccountHandler;
    }

    public async Task<string> HandleUpdateStatusChannel(CancellationToken cancellationToken)
    {
        if (_discordStatusChannelUpdateHandler == null)
            return "Discord status channel update handler is not configured properly.";

        return await _discordStatusChannelUpdateHandler.HandleStatusUpdate(cancellationToken);
    }

    public async Task<TryConnectResponse> HandleTryConnectAccountChannel(string code, ulong userId, CancellationToken cancellationToken)
    {
        if (_discordConnectAccountHandler == null)
            return new TryConnectResponse
            {
                success = false,
                message = "Discord connection handles is not configured properly."
            };

        return await _discordConnectAccountHandler.HandleConnectAccount(code, userId, cancellationToken);
    }
}
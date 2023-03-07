using Grpc.Net.Client;

namespace Realm.DiscordBot.Channels;

internal class PrivateMessagesChannels
{
    private readonly ILogger<PrivateMessagesChannels> _logger;
    private readonly Discord.PrivateMessagesChannels.PrivateMessagesChannelsClient _statusChannelClient;

    public delegate Task<string> GetStatusChannelContent();

    public PrivateMessagesChannels(ILogger<PrivateMessagesChannels> logger, GrpcChannel grpcChannel)
    {
        _statusChannelClient = new(grpcChannel);
        _logger = logger;
    }
    
    public async Task RelayPrivateMessage(ulong userId, ulong messageId, string message)
    {
        await _statusChannelClient.ReceivedPrivateMessageAsync(new SendPrivateMessageRequest
        {
            UserId = userId,
            MessageId = messageId,
            Message= message,
        });
    }
}

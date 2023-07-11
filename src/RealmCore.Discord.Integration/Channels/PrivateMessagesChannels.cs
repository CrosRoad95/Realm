using static Discord.PrivateMessagesChannels;

namespace RealmCore.Discord.Integration.Channels;

public sealed class PrivateMessagesChannels : ChannelBase
{
    private readonly ILogger<PrivateMessagesChannels> _logger;
    private readonly PrivateMessagesChannelsClient _statusChannelClient;

    public delegate Task<string> GetStatusChannelContent();

    public PrivateMessagesChannels(ILogger<PrivateMessagesChannels> logger, GrpcChannel grpcChannel, DiscordSocketClient discordSocketClient)
    {
        _statusChannelClient = new(grpcChannel);
        _logger = logger;
        discordSocketClient.MessageReceived += HandleMessageReceived;
    }

    private async Task HandleMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage.Channel is SocketDMChannel)
        {
            await _statusChannelClient.ReceivedPrivateMessageAsync(new SendPrivateMessageRequest
            {
                UserId = socketMessage.Author.Id,
                MessageId = socketMessage.Id,
                Message = socketMessage.Content,
            });
        }
    }
}

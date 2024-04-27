using static Discord.PrivateMessagesChannels;

namespace RealmCore.Discord.Integration.Channels;

public sealed class DiscordPrivateMessagesChannels : IChannelBase
{
    private readonly ILogger<DiscordPrivateMessagesChannels> _logger;
    private readonly IntegrationHeader _integrationHeader;
    private readonly PrivateMessagesChannelsClient _statusChannelClient;

    public delegate Task<string> GetStatusChannelContent();

    public DiscordPrivateMessagesChannels(ILogger<DiscordPrivateMessagesChannels> logger, GrpcChannel grpcChannel, DiscordSocketClient discordSocketClient, IntegrationHeader integrationHeader)
    {
        _statusChannelClient = new(grpcChannel);
        _logger = logger;
        _integrationHeader = integrationHeader;
        discordSocketClient.MessageReceived += HandleMessageReceived;
    }

    private async Task HandleMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage.Channel is SocketDMChannel)
        {
            await _statusChannelClient.ReceivedPrivateMessageAsync(new SendPrivateMessageRequest
            {
                Header = new Header
                {
                    Version = _integrationHeader.Version
                },
                UserId = socketMessage.Author.Id,
                MessageId = socketMessage.Id,
                Message = socketMessage.Content,
            });
        }
    }

    public Task StartAsync(SocketGuild socketGuild)
    {
        return Task.CompletedTask;
    }
}

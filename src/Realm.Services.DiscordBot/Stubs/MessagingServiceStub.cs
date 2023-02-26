using Grpc.Core;

namespace Realm.DiscordBot.Stubs;

internal class MessagingServiceStub : Messaging.MessagingBase
{
    private readonly DiscordClient _discordClient;
    private readonly ILogger<MessagingServiceStub> _logger;

    public MessagingServiceStub(DiscordClient discordClient, ILogger<MessagingServiceStub> logger)
    {
        _discordClient = discordClient;
        _logger = logger;
    }

    public override async Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
    {
        var channel = _discordClient.GetChannel(request.ChannelId) as SocketTextChannel;
        try
        {
            var message = await channel.SendMessageAsync(request.Message);
            return new SendMessageResponse
            {
                Success = true,
                MessageId = message.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message");
            return new SendMessageResponse
            {
                Success = false
            };
        }
    }
}

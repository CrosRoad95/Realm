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

    public override async Task<SendFileResponse> SendFile(SendFileRequest request, ServerCallContext context)
    {
        var channel = _discordClient.GetChannel(request.ChannelId) as SocketTextChannel;
        try
        {
            using var stream = new MemoryStream(request.File.Memory.ToArray());
            var message = await channel.SendFileAsync(stream, request.FileName, request.Message);
            return new SendFileResponse
            {
                Success = true,
                MessageId = message.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send file");
            return new SendFileResponse
            {
                Success = false
            };
        }
    }

    public override async Task<SendMessageToUserResponse> SendMessageToUser(SendMessageToUserRequest request, ServerCallContext context)
    {
        var user = _discordClient.GetUser(request.UserId);
        try
        {
            var message = await user.SendMessageAsync(request.Message);
            return new SendMessageToUserResponse
            {
                Success = true,
                MessageId = message.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to user");
            return new SendMessageToUserResponse
            {
                Success = false
            };
        }
    }
}

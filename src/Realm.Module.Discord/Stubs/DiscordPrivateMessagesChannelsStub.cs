namespace Realm.Module.Discord.Stubs;

internal class DiscordPrivateMessagesChannelsStub : PrivateMessagesChannels.PrivateMessagesChannelsBase
{
    private readonly IDiscordService _discordService;

    public DiscordPrivateMessagesChannelsStub(IDiscordService discordService)
    {
        _discordService = discordService;
    }

    public override async Task<SendPrivateMessageResponse> ReceivedPrivateMessage(SendPrivateMessageRequest request, ServerCallContext context)
    {
        if (_discordService.PrivateMessageReceived != null)
        {
            await _discordService.PrivateMessageReceived(request.UserId, request.MessageId, request.Message, context.CancellationToken);
            return new SendPrivateMessageResponse
            {
                Success = true
            };
        }

        return new SendPrivateMessageResponse
        {
            Success = false
        };
    }
}

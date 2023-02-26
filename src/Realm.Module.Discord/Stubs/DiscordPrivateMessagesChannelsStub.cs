namespace Realm.Module.Discord.Stubs;

internal class DiscordPrivateMessagesChannelsStub : PrivateMessagesChannels.PrivateMessagesChannelsBase
{
    private readonly IDiscordService _discordService;

    public DiscordPrivateMessagesChannelsStub(IDiscordService discordService)
    {
        _discordService = discordService;
    }

    public override Task<SendPrivateMessageResponse> ReceivedPrivateMessage(SendPrivateMessageRequest request, ServerCallContext context)
    {
        if (_discordService.PrivateMessageReceived != null)
        {
            _discordService.PrivateMessageReceived(request.UserId, request.MessageId, request.Message, context.CancellationToken);
            return Task.FromResult(new SendPrivateMessageResponse
            {
                Success = true
            });
        }

        return Task.FromResult(new SendPrivateMessageResponse
        {
            Success = false
        });
    }
}

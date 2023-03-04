namespace Realm.Module.Discord.Stubs;

internal class DiscordTextBasedCommandsStub : Commands.CommandsBase
{
    private readonly IDiscordService _discordService;

    public DiscordTextBasedCommandsStub(IDiscordService discordService)
    {
        _discordService = discordService;
    }

    public override async Task<SendTextBasedCommandResponse> SendTextBasedCommand(SendTextBasedCommandRequest request, ServerCallContext context)
    {
        if (_discordService.TextBasedCommandReceived != null)
        {
            await _discordService.TextBasedCommandReceived(request.UserId, request.ChannelId, request.Command, context.CancellationToken);
            return new SendTextBasedCommandResponse
            {
                Success = true
            };
        }

        return new SendTextBasedCommandResponse
        {
            Success = false
        };
    }
}

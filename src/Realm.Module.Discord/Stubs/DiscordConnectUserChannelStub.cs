namespace Realm.Module.Discord.Stubs;

internal sealed class DiscordConnectUserChannelStub : ConnectUserChannel.ConnectUserChannelBase
{
    private readonly IDiscordService _discordService;
    private readonly ILogger<DiscordConnectUserChannelStub> _logger;

    public DiscordConnectUserChannelStub(IDiscordService discordService, ILogger<DiscordConnectUserChannelStub> logger)
    {
        _discordService = discordService;
        _logger = logger;
    }

    public override async Task<SendConnectionCodeResponse> TryConnect(SendConnectionCodeRequest request, ServerCallContext context)
    {
        if (_discordService.TryConnectUserChannel == null)
            return new SendConnectionCodeResponse
            {
                Success = false,
            };

        try
        {
            var response = await _discordService.TryConnectUserChannel(request.Code, request.UserId, context.CancellationToken);
            return new SendConnectionCodeResponse
            {
                Success = response.success,
                Message = response.message,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting user.");
            return new SendConnectionCodeResponse
            {
                Success = false,
                Message = "Error connecting user.",
            };
        }
    }
}
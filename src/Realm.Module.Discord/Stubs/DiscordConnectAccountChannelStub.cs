namespace Realm.Module.Discord.Stubs;

internal sealed class DiscordConnectAccountChannelStub : ConnectAccountChannel.ConnectAccountChannelBase
{
    private readonly IDiscordService _discordService;
    private readonly ILogger<DiscordConnectAccountChannelStub> _logger;

    public DiscordConnectAccountChannelStub(IDiscordService discordService, ILogger<DiscordConnectAccountChannelStub> logger)
    {
        _discordService = discordService;
        _logger = logger;
    }

    public override async Task<SendConnectionCodeResponse> TryConnect(SendConnectionCodeRequest request, ServerCallContext context)
    {
        if (_discordService.TryConnectAccountChannel == null)
            return new SendConnectionCodeResponse
            {
                Success = false,
            };

        try
        {
            var response = await _discordService.TryConnectAccountChannel(request.Code, request.UserId, context.CancellationToken);
            return new SendConnectionCodeResponse
            {
                Success = response.success,
                Message = response.message,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting account.");
            return new SendConnectionCodeResponse
            {
                Success = false,
                Message = "Error connecting account.",
            };
        }
    }
}
namespace RealmCore.Module.Discord.Stubs;

internal sealed class DiscordStatusChannelServiceStub : StatusChannel.StatusChannelBase
{
    private readonly IDiscordService _discordService;
    private readonly ILogger<DiscordStatusChannelServiceStub> _logger;

    public DiscordStatusChannelServiceStub(IDiscordService grpcDiscord, ILogger<DiscordStatusChannelServiceStub> logger)
    {
        _discordService = grpcDiscord;
        _logger = logger;
    }

    public override async Task<ContentResponse> Update(ContentRequest request, ServerCallContext context)
    {
        if (_discordService.UpdateStatusChannel == null)
            return new ContentResponse
            {
                Message = "Discord module not configured properly."
            };

        try
        {
            var newStatus = await _discordService.UpdateStatusChannel(context.CancellationToken);
            return new ContentResponse
            {
                Message = newStatus
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating server status.");
            return new ContentResponse
            {
                Message = "Error while updating server status."
            };
        }
    }
}
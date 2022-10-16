namespace Realm.Discord;

internal class DiscordIntegration : IDiscord, IAsyncService
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordConfiguration _discordConfiguration;
    private readonly IStatusChannel _statusChannel;
    private readonly ILogger _logger;

    public DiscordIntegration(DiscordConfiguration discordConfiguration, IStatusChannel statusChannel, ILogger logger)
    {
        _client = new DiscordSocketClient();
        _discordConfiguration = discordConfiguration;
        _statusChannel = statusChannel;
        _logger = logger.ForContext<IDiscord>();
        _client.Ready += ClientReady;
        _client.Log += LogAsync;
    }

    public async Task StartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _discordConfiguration.Token);
        await _client.StartAsync();
    }

    private async Task ClientReady()
    {
        var guild = _client.GetGuild(_discordConfiguration.Guild);
        if (guild == null)
            throw new NullReferenceException(nameof(guild));

        BotIdProvider.BotId = _client.CurrentUser.Id;
        await _statusChannel.StartAsync(new DiscordGuild(guild));
    }

    private Task LogAsync(LogMessage log)
    {
        _logger.Information(log.ToString());
        return Task.CompletedTask;
    }
}
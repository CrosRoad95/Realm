namespace Realm.Discord;

internal class DiscordIntegration : IDiscord
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordConfiguration _discordConfiguration;
    private readonly StatusChannel _statusChannel;
    private readonly EventFunctions _eventFunctions;
    private readonly ILogger _logger;

    public DiscordIntegration(DiscordConfiguration discordConfiguration, StatusChannel statusChannel, ILogger logger, EventFunctions eventFunctions)
    {
        _client = new DiscordSocketClient();
        _discordConfiguration = discordConfiguration;
        _statusChannel = statusChannel;
        _eventFunctions = eventFunctions;
        _logger = logger.ForContext<IDiscord>();
        _client.Ready += ClientReady;
        _client.Log += LogAsync;
    }

    public void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {
        _eventFunctions.RegisterEvent("onDiscordStatusChannelUpdate");
        scriptingModuleInterface.AddHostType(typeof(DiscordStatusChannelUpdateContext));
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
namespace Realm.Discord.Channels;

internal class StatusChannel
{
    private readonly DiscordConfiguration.StatusChannelConfiguration? _configuration;
    private readonly IBotdIdProvider _botdIdProvider;
    private readonly EventScriptingFunctions _eventFunctions;
    private ILogger _logger;
    private IDiscordMessage? _statusDiscordMessage;

    public delegate Task<string> GetStatusChannelContent();

    public StatusChannel(DiscordConfiguration discordConfiguration, IBotdIdProvider botdIdProvider, ILogger logger, EventScriptingFunctions eventFunctions)
    {
        _configuration = discordConfiguration.StatusChannel;
        _botdIdProvider = botdIdProvider;
        _eventFunctions = eventFunctions;
        _logger = logger.ForContext<StatusChannel>();
    }

    public async Task StartAsync(IDiscordGuild discordGuild)
    {
        if (_configuration == null)
            return;

        var channel = discordGuild.GetChannelById(_configuration.ChannelId);
        _statusDiscordMessage = await channel.GetLastMessageSendByUser(_botdIdProvider.Provide());
        if (_statusDiscordMessage == null)
            _statusDiscordMessage = await channel.SendMessage(string.Empty);

        await Update();
        while(true) // TODO: user periodic timer
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            await Update();
        }
    }
    
    public async Task Update()
    {
        try
        {
            if(_statusDiscordMessage != null)
            {
                var context = new DiscordStatusChannelUpdateContext();
                await _eventFunctions.InvokeEvent("onDiscordStatusChannelUpdate", context);
                var content = context.Content;
                if (!string.IsNullOrEmpty(content))
                    await _statusDiscordMessage.Modify(content);
            }
        }
        catch(Exception ex)
        {
            _logger.Error(ex, "Failed to update status channel.");
        }
    }
}

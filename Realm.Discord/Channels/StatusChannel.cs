namespace Realm.Discord.Channels;

internal class StatusChannel : IStatusChannel
{
    private readonly DiscordConfiguration.StatusChannelConfiguration _configuration;
    private readonly IBotdIdProvider _botdIdProvider;
    private ILogger _logger;
    private IDiscordMessage? _statusDiscordMessage;

    public delegate Task<string> GetStatusChannelContent();

    public Func<Task<string>>? BeginStatusChannelUpdate { get; set; } = null;

    public StatusChannel(DiscordConfiguration discordConfiguration, IBotdIdProvider botdIdProvider, ILogger logger)
    {
        _configuration = discordConfiguration.StatusChannel;
        _botdIdProvider = botdIdProvider;
        _logger = logger.ForContext<IStatusChannel>();
    }

    public async Task StartAsync(IDiscordGuild discordGuild)
    {
        var channel = discordGuild.GetChannelById(_configuration.ChannelId);
        _statusDiscordMessage = await channel.GetLastMessageSendByUser(_botdIdProvider.Provide());
        if (_statusDiscordMessage == null)
            _statusDiscordMessage = await channel.SendMessage("");

        await Update();
        while(true)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            await Update();
        }
    }
    
    public async Task Update()
    {
        try
        {
            if(_statusDiscordMessage != null && BeginStatusChannelUpdate != null)
            {
                var content = await BeginStatusChannelUpdate();
                if(!string.IsNullOrEmpty(content))
                    await _statusDiscordMessage.Modify(content);
            }
        }
        catch(Exception ex)
        {
            _logger.Error(ex, "Failed to update status channel.");
        }
    }
}

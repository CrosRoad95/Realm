namespace Realm.Discord.Channels;

internal class StatusChannel : IStatusChannel
{
    private readonly DiscordConfiguration.StatusChannelConfiguration _configuration;
    private readonly IBotdIdProvider _botdIdProvider;
    private IDiscordMessage? _statusDiscordMessage;

    public StatusChannel(DiscordConfiguration discordConfiguration, IBotdIdProvider botdIdProvider)
    {
        _configuration = discordConfiguration.StatusChannel;
        _botdIdProvider = botdIdProvider;
    }

    public async Task StartAsync(IDiscordGuild discordGuild)
    {
        var channel = discordGuild.GetChannelById(_configuration.ChannelId);
        _statusDiscordMessage = await channel.GetLastMessageSendByUser(_botdIdProvider.Provide());
        if (_statusDiscordMessage == null)
            _statusDiscordMessage = await channel.SendMessage("");
        await Update();
    }
    
    public async Task Update()
    {
        if(_statusDiscordMessage != null)
            await _statusDiscordMessage.Modify($"Ostatnia aktualizacja statusu serwera: {DateTime.Now}");
    }
}

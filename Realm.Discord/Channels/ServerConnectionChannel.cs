using Realm.Scripting.Functions;

namespace Realm.Discord.Channels;

internal class ServerConnectionChannel
{
    private readonly DiscordConfiguration.ConnectServerAccountConfiguration? _configuration;
    private readonly IBotdIdProvider _botdIdProvider;
    private readonly EventScriptingFunctions _eventFunctions;
    private ILogger _logger;
    private IDiscordMessage? _informationMessage;

    public delegate Task<string> GetStatusChannelContent();

    public ServerConnectionChannel(DiscordConfiguration discordConfiguration, IBotdIdProvider botdIdProvider, ILogger logger, EventScriptingFunctions eventFunctions)
    {
        _configuration = discordConfiguration.ConnectServerAccountChannel;
        _botdIdProvider = botdIdProvider;
        _eventFunctions = eventFunctions;
        _logger = logger.ForContext<ServerConnectionChannel>();
    }

    public async Task StartAsync(IDiscordGuild discordGuild)
    {
        if (_configuration == null)
            return;
        var channel = discordGuild.GetChannelById(_configuration.ChannelId);
        _informationMessage = await channel.GetLastMessageSendByUser(_botdIdProvider.Provide());
        if (_informationMessage == null)
        {
            _informationMessage = await channel.SendMessage(_configuration.InformationMessage);
        }
        else
        {
            await _informationMessage.Modify(_configuration.InformationMessage); // TODO: Update only if message changed
        }
    }
}

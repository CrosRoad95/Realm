using Grpc.Net.Client;

namespace Realm.Services.DiscordBot.Channels;

internal class DiscordServerConnectionChannel
{
    private readonly DiscordConfiguration.ConnectServerAccountConfiguration? _configuration;
    private readonly BotIdProvider _botdIdProvider;
    private readonly GrpcChannel _grpcChannel;
    private ILogger _logger;
    private DiscordMessage? _informationMessage;

    private readonly Discord.StatusChannel.StatusChannelClient _statusChannelClient;

    public delegate Task<string> GetStatusChannelContent();

    public DiscordServerConnectionChannel(DiscordConfiguration discordConfiguration, BotIdProvider botdIdProvider, ILogger logger, GrpcChannel grpcChannel)
    {
        _configuration = discordConfiguration.ConnectServerAccountChannel;
        _botdIdProvider = botdIdProvider;
        _grpcChannel = grpcChannel;
        _logger = logger.ForContext<DiscordServerConnectionChannel>();
        _statusChannelClient = new Discord.StatusChannel.StatusChannelClient(grpcChannel);
    }

    public async Task StartAsync(DiscordGuild discordGuild)
    {
        if (_configuration == null)
            return;
        var channel = discordGuild.GetChannelById(_configuration.ChannelId);
        _informationMessage = await channel.GetLastMessageSendByUser(_botdIdProvider.Id);
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

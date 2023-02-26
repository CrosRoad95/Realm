using Grpc.Net.Client;

namespace Realm.DiscordBot;

internal class DiscordClient
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordBotConfiguration _discordConfiguration;
    private readonly DiscordStatusChannel _discordStatusChannel;
    private readonly CommandHandler _commandHandler;
    private readonly BotIdProvider _botIdProvider;
    private readonly ILogger<DiscordClient> _logger;
    private SocketGuild? _socketGuild;

    public DiscordClient(DiscordSocketClient discordSocketClient, DiscordBotConfiguration discordConfiguration, DiscordStatusChannel discordStatusChannel,
        ILogger<DiscordClient> logger, CommandHandler commandHandler, BotIdProvider botIdProvider, GrpcChannel grpcChannel)
    {
        _client = discordSocketClient;
        _discordConfiguration = discordConfiguration;
        _discordStatusChannel = discordStatusChannel;
        _commandHandler = commandHandler;
        _botIdProvider = botIdProvider;
        _logger = logger;
        _client.Ready += HandleReady;
        _client.Log += HandleLog;
        _client.GuildMemberUpdated += HandleGuildMemberUpdated;
    }

    private async Task HandleGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser sockerGuildUser)
    {
        ;
        //await _discordUserChangedHandler.Handle(new DiscordUser(sockerGuildUser));
    }

    public async Task StartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _discordConfiguration.Token);
        await _client.StartAsync();
    }

    private async Task HandleReady()
    {
        _socketGuild = _client.GetGuild(_discordConfiguration.Guild);
        if (_socketGuild == null)
            throw new NullReferenceException(nameof(_socketGuild));

        _botIdProvider.Id = _client.CurrentUser.Id;
        _ = Task.Run(async () => await _discordStatusChannel.StartAsync(_socketGuild));
        await _socketGuild.DownloadUsersAsync();

        await _commandHandler.InitializeAsync();
    }

    public SocketGuildChannel GetChannel(ulong channelId)
    {
        if(_socketGuild == null)
            throw new NullReferenceException(nameof(_socketGuild));

        return _socketGuild.GetChannel(channelId);
    }
    
    public SocketGuildUser GetUser(ulong userId)
    {
        if(_socketGuild == null)
            throw new NullReferenceException(nameof(_socketGuild));

        return _socketGuild.GetUser(userId);
    }

    private Task HandleLog(LogMessage log)
    {
        _logger.LogInformation(log.ToString());
        return Task.CompletedTask;
    }
}
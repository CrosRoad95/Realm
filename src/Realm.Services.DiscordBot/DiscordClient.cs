using Grpc.Net.Client;

namespace Realm.DiscordBot;

internal class DiscordClient
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordConfiguration _discordConfiguration;
    private readonly DiscordStatusChannel _discordStatusChannel;
    private readonly DiscordServerConnectionChannel _serverConnectionChannel;
    private readonly CommandHandler _commandHandler;
    private readonly BotIdProvider _botIdProvider;
    private readonly ILogger _logger;
    private DiscordGuild? _discordGuild = null;

    private readonly Handshake.HandshakeClient _handshakeClient;

    public DiscordClient(DiscordSocketClient discordSocketClient, DiscordConfiguration discordConfiguration, DiscordStatusChannel discordStatusChannel, DiscordServerConnectionChannel serverConnectionChannel,
        ILogger logger, CommandHandler commandHandler, BotIdProvider botIdProvider, GrpcChannel grpcChannel)
    {
        _client = discordSocketClient;
        _discordConfiguration = discordConfiguration;
        _discordStatusChannel = discordStatusChannel;
        _serverConnectionChannel = serverConnectionChannel;
        _commandHandler = commandHandler;
        _botIdProvider = botIdProvider;
        _logger = logger;
        _client.Ready += HandleReady;
        _client.Log += HandleLog;
        _client.GuildMemberUpdated += HandleGuildMemberUpdated;
        _handshakeClient = new(grpcChannel);
    }

    private async Task HandleGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser sockerGuildUser)
    {
        //await _discordUserChangedHandler.Handle(new DiscordUser(sockerGuildUser));
    }

    public async Task StartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _discordConfiguration.Token);
        await _client.StartAsync();
    }

    private async Task HandleReady()
    {
        var guild = _client.GetGuild(_discordConfiguration.Guild);
        if (guild == null)
            throw new NullReferenceException(nameof(guild));

        _botIdProvider.Id = _client.CurrentUser.Id;
        _discordGuild = new DiscordGuild(guild);
        _ = Task.Run(async () => await _discordStatusChannel.StartAsync(_discordGuild));
        _ = Task.Run(async () => await _serverConnectionChannel.StartAsync(_discordGuild));

        await _commandHandler.InitializeAsync();
    }

    private Task HandleLog(LogMessage log)
    {
        _logger.Information(log.ToString());
        return Task.CompletedTask;
    }
}
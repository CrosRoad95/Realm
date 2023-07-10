namespace RealmCore.Discord.Integration;

internal class RealmDiscordClient : IRealmDiscordClient
{
    private readonly DiscordSocketClient _client;
    private readonly IOptions<DiscordBotOptions> _discordBotOptions;
    private readonly ChannelBase[] _channels;
    private readonly CommandHandler _commandHandler;
    private readonly BotIdProvider _botIdProvider;
    private readonly TextBasedCommands _textBasedCommands;
    private SocketGuild? _socketGuild;
    public event Action? Ready;

    public RealmDiscordClient(DiscordSocketClient discordSocketClient, IOptions<DiscordBotOptions> discordBotOptions, IEnumerable<ChannelBase> channels, CommandHandler commandHandler, BotIdProvider botIdProvider, TextBasedCommands textBasedCommands,
        IDiscordLogger discordLogger)
    {
        _client = discordSocketClient;
        _discordBotOptions = discordBotOptions;
        _channels = channels.ToArray();
        _commandHandler = commandHandler;
        _botIdProvider = botIdProvider;
        _textBasedCommands = textBasedCommands;
        discordLogger.Attach(_client);
        _client.Ready += HandleReady;
        _client.GuildMemberUpdated += HandleGuildMemberUpdated;
        _client.MessageReceived += HandleMessageReceived;
    }

    private async Task HandleMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage.Author.IsBot || !socketMessage.Content.Any())
            return;

        if (socketMessage.Content.StartsWith(_discordBotOptions.Value.TextBasedCommandPrefix))
            await _textBasedCommands.Relay(socketMessage);
    }

    private async Task HandleGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser sockerGuildUser)
    {
        ;
        //await _discordUserChangedHandler.Handle(new DiscordUser(sockerGuildUser));
    }

    public async Task StartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _discordBotOptions.Value.Token);
        await _client.StartAsync();
    }

    private async Task HandleReady()
    {
        _socketGuild = _client.GetGuild(_discordBotOptions.Value.Guild);
        if (_socketGuild == null)
            throw new NullReferenceException(nameof(_socketGuild));

        _botIdProvider.Id = _client.CurrentUser.Id;
        foreach (var item in _channels)
        {
            _ = Task.Run(async () => await item.StartAsync(_socketGuild));
        }
        await _socketGuild.DownloadUsersAsync();

        await _commandHandler.InitializeAsync();
        Ready?.Invoke();
    }

    public SocketGuildChannel GetChannel(ulong channelId)
    {
        if (_socketGuild == null)
            throw new NullReferenceException(nameof(_socketGuild));

        return _socketGuild.GetChannel(channelId);
    }

    public SocketGuildUser GetUser(ulong userId)
    {
        if (_socketGuild == null)
            throw new NullReferenceException(nameof(_socketGuild));

        return _socketGuild.GetUser(userId);
    }
}
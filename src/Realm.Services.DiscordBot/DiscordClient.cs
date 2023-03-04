using Discord.WebSocket;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using Realm.DiscordBot.Services;

namespace Realm.DiscordBot;

internal class DiscordClient
{
    private readonly DiscordSocketClient _client;
    private readonly IOptions<DiscordBotOptions> _discordBotOptions;
    private readonly DiscordStatusChannel _discordStatusChannel;
    private readonly Channels.PrivateMessagesChannels _privateMessagesChannels;
    private readonly CommandHandler _commandHandler;
    private readonly BotIdProvider _botIdProvider;
    private readonly TextBasedCommands _textBasecCommands;
    private readonly ILogger<DiscordClient> _logger;
    private SocketGuild? _socketGuild;

    public DiscordClient(DiscordSocketClient discordSocketClient, IOptions<DiscordBotOptions> discordBotOptions, DiscordStatusChannel discordStatusChannel,
        Channels.PrivateMessagesChannels privateMessagesChannels,
        ILogger<DiscordClient> logger, CommandHandler commandHandler, BotIdProvider botIdProvider, GrpcChannel grpcChannel, TextBasedCommands textBasecCommands)
    {
        _client = discordSocketClient;
        _discordBotOptions = discordBotOptions;
        _discordStatusChannel = discordStatusChannel;
        _privateMessagesChannels = privateMessagesChannels;
        _commandHandler = commandHandler;
        _botIdProvider = botIdProvider;
        _textBasecCommands = textBasecCommands;
        _logger = logger;
        _client.Ready += HandleReady;
        _client.Log += HandleLog;
        _client.GuildMemberUpdated += HandleGuildMemberUpdated;
        _client.MessageReceived += HandlePrivateMessageReceived;
    }

    private async Task HandlePrivateMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage.Author.IsBot || !socketMessage.Content.Any())
            return;

        if (socketMessage.Content.StartsWith("&"))
            await _textBasecCommands.Relay(socketMessage);

        if (socketMessage.Channel is SocketDMChannel)
            await _privateMessagesChannels.RelayPrivateMessage(socketMessage.Author.Id, socketMessage.Id, socketMessage.Content);
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
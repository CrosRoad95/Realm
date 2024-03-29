﻿global using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace RealmCore.Discord.Integration;

internal class RealmDiscordClient : IRealmDiscordClient
{
    private readonly DiscordSocketClient _client;
    private readonly IOptions<DiscordBotOptions> _discordBotOptions;
    private readonly ChannelBase[] _channels;
    private readonly DiscordService[] _discordServices;
    private readonly CommandHandler _commandHandler;
    private readonly BotIdProvider _botIdProvider;
    private readonly TextBasedCommands _textBasedCommands;

    public event Action? Ready;

    public RealmDiscordClient(DiscordSocketClient discordSocketClient, IOptions<DiscordBotOptions> discordBotOptions, IEnumerable<ChannelBase> channels, IEnumerable<DiscordService> discordServices, CommandHandler commandHandler, BotIdProvider botIdProvider, TextBasedCommands textBasedCommands)
    {
        _client = discordSocketClient;
        _discordBotOptions = discordBotOptions;
        _channels = channels.ToArray();
        _discordServices = discordServices.ToArray();
        _commandHandler = commandHandler;
        _botIdProvider = botIdProvider;
        _textBasedCommands = textBasedCommands;
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

    public SocketGuild GetGuild() => _client.GetGuild(_discordBotOptions.Value.Guild);

    private async Task HandleReady()
    {
        var socketGuild = GetGuild();
        if (socketGuild == null)
            throw new NullReferenceException(nameof(socketGuild));

        _botIdProvider.Id = _client.CurrentUser.Id;

        await socketGuild.DownloadUsersAsync();

        await _commandHandler.InitializeAsync();

        foreach (var item in _channels)
        {
            _ = Task.Run(async () => await item.StartAsync(socketGuild));
        }
        foreach (var item in _discordServices)
        {
            _ = Task.Run(async () => await item.StartAsync(socketGuild));
        }
        Ready?.Invoke();
    }

    public SocketGuildChannel GetChannel(ulong channelId)
    {
        var socketGuild = GetGuild();
        if (socketGuild == null)
            throw new NullReferenceException(nameof(socketGuild));

        return socketGuild.GetChannel(channelId);
    }

    public SocketGuildUser GetUser(ulong userId)
    {
        var socketGuild = GetGuild();
        if (socketGuild == null)
            throw new NullReferenceException(nameof(socketGuild));

        return socketGuild.GetUser(userId);
    }
}
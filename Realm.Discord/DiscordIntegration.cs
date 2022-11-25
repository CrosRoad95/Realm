using Realm.Discord.Interfaces;
using Realm.Scripting.Functions;

namespace Realm.Discord;

internal class DiscordIntegration : IDiscord
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordConfiguration _discordConfiguration;
    private readonly StatusChannel _statusChannel;
    private readonly ServerConnectionChannel _serverConnectionChannel;
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly CommandHandler _commandHandler;
    private readonly IDiscordUserChangedHandler _discordUserChangedHandler;
    private readonly ILogger _logger;
    private IDiscordGuild? _discordGuild = null;

    public DiscordIntegration(DiscordSocketClient discordSocketClient, DiscordConfiguration discordConfiguration, StatusChannel statusChannel, ServerConnectionChannel serverConnectionChannel,
        ILogger logger, EventScriptingFunctions eventFunctions, CommandHandler commandHandler, IDiscordUserChangedHandler discordUserChangedHandler)
    {
        _client = discordSocketClient;
        _discordConfiguration = discordConfiguration;
        _statusChannel = statusChannel;
        _serverConnectionChannel = serverConnectionChannel;
        _eventFunctions = eventFunctions;
        _commandHandler = commandHandler;
        _discordUserChangedHandler = discordUserChangedHandler;
        _logger = logger.ForContext<IDiscord>();
        _client.Ready += ClientReady;
        _client.Log += LogAsync;
        _client.GuildMemberUpdated += GuildMemberUpdated;
    }

    private async Task GuildMemberUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser sockerGuildUser)
    {
        await _discordUserChangedHandler.Handle(new DiscordUser(sockerGuildUser));
    }

    public void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {
        _eventFunctions.RegisterEvent(DiscordStatusChannelUpdateContext.EventName);
        scriptingModuleInterface.AddHostType(typeof(DiscordStatusChannelUpdateContext));
    }

    public async Task StartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _discordConfiguration.Token);
        await _client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private async Task ClientReady()
    {
        var guild = _client.GetGuild(_discordConfiguration.Guild);
        if (guild == null)
            throw new NullReferenceException(nameof(guild));

        BotIdProvider.BotId = _client.CurrentUser.Id;
        _discordGuild = new DiscordGuild(guild);
        _ = Task.Run(async () => await _statusChannel.StartAsync(_discordGuild));
        _ = Task.Run(async () => await _serverConnectionChannel.StartAsync(_discordGuild));

        await _commandHandler.InitializeAsync();
    }

    private Task LogAsync(LogMessage log)
    {
        _logger.Information(log.ToString());
        return Task.CompletedTask;
    }

    public IDiscordGuild? GetGuild() => _discordGuild;
}
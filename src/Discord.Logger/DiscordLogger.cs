using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace RealmCore.Discord.Logger;

internal class DiscordLogger : IDiscordLogger
{
    private readonly ILogger<DiscordLogger> _logger;

    public DiscordLogger(DiscordSocketClient discordSocketClient, ILogger<DiscordLogger> logger)
    {
        _logger = logger;
    }

    public void Attach(DiscordSocketClient discordSocketClient)
    {
        discordSocketClient.Log += HandleLog;
        discordSocketClient.Ready += HandleReady;
        discordSocketClient.MessageReceived += HandleMessageReceived;
        discordSocketClient.MessageDeleted += HandleMessageDeleted;
        discordSocketClient.UserJoined += HandleUserJoined;
        discordSocketClient.UserLeft += HandleUserLeft;
        discordSocketClient.ReactionAdded += HandleReactionAdded;
        discordSocketClient.ReactionRemoved += HandleReactionRemoved;
        discordSocketClient.InviteCreated += HandleInviteCreated;
    }

    private Task HandleInviteCreated(SocketInvite socketInvite)
    {
        throw new NotImplementedException();
    }

    private async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> userMessageCache, Cacheable<IMessageChannel, ulong> messageChannelCache, SocketReaction socketReaction)
    {
        var userMessage = await userMessageCache.GetOrDownloadAsync();
        var messageChannel = await messageChannelCache.GetOrDownloadAsync();
        var logProperties = new Dictionary<string, object>
        {
            ["discordGuild"] = ((SocketGuildChannel)messageChannel).Guild.Id.ToString(),
            ["discordUserId"] = userMessage.Id.ToString(),
            ["discordMessageId"] = messageChannel.Id.ToString(),
            ["discordEmoteName"] = socketReaction.Emote.Name,
            ["discordUsername"] = userMessage.Author.Username,
            ["discordUserDiscriminator"] = userMessage.Author.Discriminator
        };

        using var _ = _logger.BeginScope(logProperties);

        _logger.LogInformation("Reaction {discordEmoteName} removed by {discordUsername}#{discordUserDiscriminator}", logProperties["discordEmoteName"], logProperties["discordUsername"], logProperties["discordUserDiscriminator"]);
    }

    private async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> userMessageCache, Cacheable<IMessageChannel, ulong> messageChannelCache, SocketReaction socketReaction)
    {
        var userMessage = await userMessageCache.GetOrDownloadAsync().ConfigureAwait(false);
        var messageChannel = await messageChannelCache.GetOrDownloadAsync().ConfigureAwait(false);
        var logProperties = new Dictionary<string, object>
        {
            ["discordGuild"] = ((SocketGuildChannel)messageChannel).Guild.Id.ToString(),
            ["discordUserId"] = userMessage.Id.ToString(),
            ["discordMessageId"] = messageChannel.Id.ToString(),
            ["discordEmoteName"] = socketReaction.Emote.Name,
            ["discordUsername"] = userMessage.Author.Username,
            ["discordUserDiscriminator"] = userMessage.Author.Discriminator
        };

        using var _ = _logger.BeginScope(logProperties);

        _logger.LogInformation("Reaction {discordEmoteName} added by {discordUsername}#{discordUserDiscriminator}", logProperties["discordEmoteName"], logProperties["discordUsername"], logProperties["discordUserDiscriminator"]);
    }

    private Task HandleUserLeft(SocketGuild guild, SocketUser user)
    {
        var logProperties = new Dictionary<string, object>
        {
            ["discordGuild"] = guild.Id.ToString(),
            ["discordUserId"] = user.Id.ToString(),
            ["discordUsername"] = user.Username,
            ["discordUserDiscriminator"] = user.Discriminator
        };

        using var _ = _logger.BeginScope(logProperties);

        _logger.LogInformation("User {discordUsername}#{discordUserDiscriminator} left", logProperties["discordUsername"], logProperties["discordUserDiscriminator"]);

        return Task.CompletedTask;
    }

    private Task HandleUserJoined(SocketGuildUser user)
    {
        var logProperties = new Dictionary<string, object>
        {
            ["discordGuild"] = user.Guild.Id.ToString(),
            ["discordUserId"] = user.Id.ToString(),
            ["discordUsername"] = user.Username,
            ["discordUserDiscriminator"] = user.Discriminator
        };

        using var _ = _logger.BeginScope(logProperties);

        _logger.LogInformation("User {discordUsername}#{discordUserDiscriminator} joined", logProperties["discordUsername"], logProperties["discordUserDiscriminator"]);

        return Task.CompletedTask;
    }

    private async Task HandleMessageDeleted(Cacheable<IMessage, ulong> messageCache, Cacheable<IMessageChannel, ulong> messageChannelCache)
    {
        var message = await messageCache.GetOrDownloadAsync().ConfigureAwait(false);
        var messageChannel = await messageChannelCache.GetOrDownloadAsync().ConfigureAwait(false);
        var logProperties = new Dictionary<string, object>
        {
            ["discordGuild"] = ((SocketGuildChannel)messageChannel).Guild.Id.ToString(),
            ["discordMessageId"] = message.Id.ToString(),
            ["discordChannelId"] = messageChannel.Id.ToString()
        };

        using var _ = _logger.BeginScope(logProperties);

        _logger.LogInformation("Message {discordMessageId} deleted", logProperties["discordMessageId"]);
    }

    private Task HandleMessageReceived(SocketMessage message)
    {
        var logProperties = new Dictionary<string, object>
        {
            ["discordGuild"] = ((SocketGuildChannel)message.Channel).Guild.Id.ToString(),
            ["discordMessageId"] = message.Id.ToString(),
            ["discordChannelId"] = message.Channel.Id.ToString(),
            ["discordUserId"] = message.Author.Id.ToString(),
            ["discordUsername"] = message.Author.Username,
            ["discordUserDiscriminator"] = message.Author.Discriminator,
            ["discordMessage"] = message.Content
        };

        using var _ = _logger.BeginScope(logProperties);

        _logger.LogInformation("{discordUsername}#{discordUserDiscriminator}: {discordMessage}", logProperties["discordUsername"], logProperties["discordUserDiscriminator"], logProperties["discordMessage"]);

        return Task.CompletedTask;
    }

    private Task HandleReady()
    {
        _logger.LogInformation("Discord socket client ready");
        return Task.CompletedTask;
    }

    private Task HandleLog(LogMessage log)
    {
        _logger.LogInformation(log.ToString());
        return Task.CompletedTask;
    }
}

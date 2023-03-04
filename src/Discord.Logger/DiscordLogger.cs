using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Discord.Logger;

internal class DiscordLogger : IDiscordLogger
{
    private readonly ILogger<DiscordLogger> _logger;

    public DiscordLogger(ILogger<DiscordLogger> logger)
    {
        _logger = logger;
    }

    public void Attach(DiscordSocketClient discordSocketClient)
    {
        discordSocketClient.Log += HandleLog;
        discordSocketClient.Ready += HandleReady;
        discordSocketClient.MessageReceived += HandleMessageReceived;
        discordSocketClient.MessageDeleted += HandleMessageDeleted;
    }

    private Task HandleMessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> messageChannel)
    {
        _logger.LogInformation("Message id: {messageId} deleted", message.Id.ToString());
        return Task.CompletedTask;
    }

    private Task HandleReady()
    {
        _logger.LogInformation("Discord socket client ready");
        return Task.CompletedTask;
    }

    private Task HandleMessageReceived(SocketMessage message)
    {
        using var _ = _logger.BeginScope(new Dictionary<string, object>
        {
            ["messageId"] = message.Id.ToString(),
            ["channelId"] = message.Channel.Id.ToString(),
            ["discordUserId"] = message.Author.Id.ToString(),
        });
        _logger.LogInformation("{username}#{discriminator}: {message}", message.Author.Username, message.Author.Discriminator, message.Content);
        return Task.CompletedTask;
    }

    private Task HandleLog(LogMessage log)
    {
        _logger.LogInformation(log.ToString());
        return Task.CompletedTask;
    }
}

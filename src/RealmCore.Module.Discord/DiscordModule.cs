using RealmCore.Interfaces.Extend;

namespace RealmCore.Module.Discord;

public interface IDiscordHandler
{

}

internal class DiscordModule : IExternalModule
{
    private readonly ILogger<DiscordModule> _logger;

    public DiscordModule(ILogger<DiscordModule> logger, IDiscordService discordService, IEnumerable<IDiscordHandler>? handlers = null)
    {
        _logger = logger;

        if (handlers == null)
        {
            _logger.LogInformation("{module} module loaded with no handlers", "Discord");
            return;
        }
        _logger.LogInformation("{module} module loaded with {discordHandlersCount}", "Discord", handlers.Count());

        foreach (var handler in handlers)
        {
            switch (handler)
            {
                case IDiscordStatusChannelUpdateHandler discordStatusChannelUpdateHandler:
                    discordService.UpdateStatusChannel = discordStatusChannelUpdateHandler.HandleStatusUpdate;
                    break;
                case IDiscordConnectUserHandler discordConnectUserHandler:
                    discordService.TryConnectUserChannel = discordConnectUserHandler.HandleConnectUser;
                    break;
                case IDiscordPrivateMessageReceivedHandler privateMessageReceivedHandler:
                    discordService.PrivateMessageReceived = privateMessageReceivedHandler.HandlePrivateMessage;
                    break;
                case IDiscordTextBasedCommandHandler discordTextBasedCommandHandler:
                    discordService.TextBasedCommandReceived = discordTextBasedCommandHandler.HandleTextCommand;
                    break;
            }
        }
    }
}
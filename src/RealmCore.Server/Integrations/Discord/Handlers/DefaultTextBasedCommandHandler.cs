namespace RealmCore.Server.Integrations.Discord.Handlers;

public class DefaultTextBasedCommandHandler : IDiscordTextBasedCommandHandler
{
    private readonly IDiscordService _discordService;

    public DefaultTextBasedCommandHandler(IDiscordService discordService)
    {
        _discordService = discordService;
    }

    public async Task HandleTextCommand(ulong userId, ulong channelId, string command)
    {
        await _discordService.HandleTextBasedCommand(userId, channelId, command[1..]);
    }
}

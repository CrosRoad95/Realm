﻿namespace RealmCore.Console.Integrations.Discord.Handlers;

public class TextBasedCommandHandler : IDiscordTextBasedCommandHandler
{
    private readonly IDiscordService _discordService;

    public TextBasedCommandHandler(IDiscordService discordService)
    {
        _discordService = discordService;
    }

    public async Task HandleTextCommand(ulong userId, ulong channelId, string command)
    {
        await _discordService.HandleTextBasedCommand(userId, channelId, command[1..]);
    }
}
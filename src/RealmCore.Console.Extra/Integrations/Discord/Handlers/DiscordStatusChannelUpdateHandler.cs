﻿using RealmCore.Module.Discord.Interfaces;
using RealmCore.Server.Modules.Server;

namespace RealmCore.Console.Extra.Integrations.Discord.Handlers;

public class DiscordStatusChannelUpdateHandler : IDiscordStatusChannelUpdateHandler
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public DiscordStatusChannelUpdateHandler(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<string> HandleStatusUpdate(CancellationToken cancellationToken)
    {
        return Task.FromResult($"test 123 {_dateTimeProvider.Now}");
    }
}

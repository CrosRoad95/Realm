﻿namespace Realm.Discord.Classes;

internal class DiscordGuild : IDiscordGuild
{
    private readonly SocketGuild _socketGuild;

    public DiscordGuild(SocketGuild socketGuild)
    {
        _socketGuild = socketGuild;
    }

    public IDiscordChannel GetChannelById(ulong id)
    {
        return new DiscordChannel(_socketGuild.GetChannel(id));
    }
}
namespace Realm.Services.DiscordBot.Classes;

internal class DiscordGuild
{
    private readonly SocketGuild _socketGuild;

    public DiscordGuild(SocketGuild socketGuild)
    {
        _socketGuild = socketGuild;
    }

    public DiscordChannel GetChannelById(ulong id)
    {
        return new DiscordChannel(_socketGuild.GetChannel(id));
    }

    public DiscordUser GetUserById(ulong id)
    {
        return new DiscordUser(_socketGuild.GetUser(id));
    }
}

namespace Realm.Interfaces.Discord;

public interface IDiscordGuild
{
    IDiscordChannel GetChannelById(ulong id);
}

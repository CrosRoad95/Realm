namespace Realm.Discord.Channels.Interfaces;

public interface IStatusChannel
{
    Task StartAsync(IDiscordGuild discordGuild);
}

namespace Realm.Interfaces.Discord;

public interface IStatusChannel
{
    Task StartAsync(IDiscordGuild discordGuild);
}

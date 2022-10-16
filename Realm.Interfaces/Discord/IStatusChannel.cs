namespace Realm.Interfaces.Discord;

public interface IStatusChannel
{
    Func<Task<string>> BeginStatusChannelUpdate { get; set; }

    Task StartAsync(IDiscordGuild discordGuild);
}

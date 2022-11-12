namespace Realm.Interfaces.Discord;

public interface IDiscord
{
    IDiscordGuild? GetGuild();
    ValueTask<IDiscordUser?> GetUserAsync(ulong id);
}

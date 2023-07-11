using Discord.WebSocket;

namespace RealmCore.Discord.Logger;

public interface IDiscordLogger
{
    void Attach(DiscordSocketClient discordSocketClient);
}

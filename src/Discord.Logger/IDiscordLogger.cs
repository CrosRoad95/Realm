using Discord.WebSocket;

namespace Discord.Logger;

public interface IDiscordLogger
{
    void Attach(DiscordSocketClient discordSocketClient);
}

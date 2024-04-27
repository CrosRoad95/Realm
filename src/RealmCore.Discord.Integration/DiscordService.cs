namespace RealmCore.Discord.Integration;

public interface IDiscordService
{
    Task StartAsync(SocketGuild socketGuild);
}

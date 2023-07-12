namespace RealmCore.Discord.Integration;

public abstract class DiscordService
{
    public virtual Task StartAsync(SocketGuild socketGuild) { return Task.CompletedTask; }
}

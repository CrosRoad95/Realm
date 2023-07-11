namespace RealmCore.Discord.Integration.Channels;

public abstract class ChannelBase
{
    public virtual Task StartAsync(SocketGuild socketGuild) { return Task.CompletedTask; }
}

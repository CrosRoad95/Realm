namespace RealmCore.Discord.Integration.Channels;

public interface IChannelBase
{
    Task StartAsync(SocketGuild socketGuild);
}

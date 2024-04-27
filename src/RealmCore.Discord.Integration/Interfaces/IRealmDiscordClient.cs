
namespace RealmCore.Discord.Integration.Interfaces;

public interface IRealmDiscordClient
{
    event Action? Ready;

    SocketGuildChannel GetChannel(ulong channelId);
    SocketGuildUser GetUser(ulong userId);
    Task StartAsync();
}

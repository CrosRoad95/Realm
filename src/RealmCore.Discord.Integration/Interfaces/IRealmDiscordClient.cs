namespace RealmCore.Discord.Integration.Interfaces;

public interface IRealmDiscordClient
{
    event Action? Ready;

    Task StartAsync();
}

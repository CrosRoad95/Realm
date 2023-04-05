namespace Realm.Module.Discord.Interfaces;

public interface IDiscordConnectUserHandler
{
    Task<TryConnectResponse> HandleConnectUser(string code, ulong userId, CancellationToken cancellationToken);
}

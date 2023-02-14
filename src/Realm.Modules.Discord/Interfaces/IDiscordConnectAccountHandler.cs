namespace Realm.Module.Discord.Interfaces;

public interface IDiscordConnectAccountHandler
{
    Task<TryConnectResponse> HandleConnectAccount(string code, ulong userId, CancellationToken cancellationToken);
}

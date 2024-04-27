namespace RealmCore.Module.Discord.Interfaces;

public interface IDiscordConnectUserHandler : IDiscordHandler
{
    Task<TryConnectResponse> HandleConnectUser(string code, ulong userId, CancellationToken cancellationToken);
}

namespace RealmCore.Module.Discord.Interfaces;

public interface IDiscordStatusChannelUpdateHandler : IDiscordHandler
{
    Task<string> HandleStatusUpdate(CancellationToken cancellationToken);
}

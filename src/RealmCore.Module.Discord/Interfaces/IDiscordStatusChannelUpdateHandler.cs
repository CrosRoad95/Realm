namespace RealmCore.Module.Discord.Interfaces;

public interface IDiscordStatusChannelUpdateHandler
{
    Task<string> HandleStatusUpdate(CancellationToken cancellationToken);
}

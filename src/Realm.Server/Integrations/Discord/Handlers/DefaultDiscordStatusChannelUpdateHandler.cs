using Realm.Module.Discord.Interfaces;

namespace Realm.Server.Integrations.Discord.Handlers;

public class DefaultDiscordStatusChannelUpdateHandler : IDiscordStatusChannelUpdateHandler
{
    public Task<string> HandleStatusUpdate(CancellationToken cancellationToken)
    {
        return Task.FromResult($"test 123 {DateTime.Now}");
    }
}

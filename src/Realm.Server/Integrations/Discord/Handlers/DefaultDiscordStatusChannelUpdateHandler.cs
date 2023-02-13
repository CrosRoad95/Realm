using Realm.Module.Discord.Interfaces;

namespace Realm.Server.Integrations.Discord.Handlers;

public class DefaultDiscordStatusChannelUpdateHandler : IDiscordStatusChannelUpdateHandler
{
    public async Task<string> HandleStatusUpdate(CancellationToken cancellationToken)
    {
        return $"test 123 {DateTime.Now}";
    }
}

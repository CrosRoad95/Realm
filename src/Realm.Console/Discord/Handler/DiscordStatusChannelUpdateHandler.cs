using Realm.Module.Discord.Interfaces;

namespace Realm.Console.Discord.Handler;

internal class DiscordStatusChannelUpdateHandler : IDiscordStatusChannelUpdateHandler
{
    public async Task<string> HandleStatusUpdate()
    {
        return $"test 123 {DateTime.Now}";
    }
}

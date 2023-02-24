using Realm.Interfaces.Extend;
using Realm.Module.Discord.Interfaces;
using Realm.Module.Grpc.Services;

namespace Realm.Module.Discord;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDiscordService, DiscordService>();
        serviceCollection.AddSingleton<IModule, DiscordModule>();
        return serviceCollection;
    }
}

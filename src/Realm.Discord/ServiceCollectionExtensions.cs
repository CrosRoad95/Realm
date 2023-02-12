using Realm.Interfaces.Extend;

namespace Realm.Module.Discord;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IModule, DiscordModule>();
        return serviceCollection;
    }
}

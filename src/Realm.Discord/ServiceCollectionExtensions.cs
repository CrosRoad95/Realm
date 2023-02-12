namespace Realm.Module.Discord;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<DiscordIntegration>();
        return serviceCollection;
    }
}

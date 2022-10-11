namespace Realm.Discord;

public static class PersistanceExtensions
{
    public static IServiceCollection AddDiscord(this IServiceCollection services)
    {
        services.AddSingleton(x => x.GetRequiredService<ConfigurationProvider>().Get<DiscordConfiguration>("discord"));
        services.AddSingleton<IDiscord, DiscordIntegration>();
        services.AddSingleton<IAsyncService, DiscordIntegration>();
        services.AddSingleton<IStatusChannel, StatusChannel>();
        services.AddSingleton<IBotdIdProvider, BotIdProvider>();
        return services;
    }
}

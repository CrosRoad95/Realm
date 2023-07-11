using Microsoft.Extensions.DependencyInjection;

namespace RealmCore.Discord.Logger;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordLogger(this IServiceCollection services)
    {
        services.AddSingleton<IDiscordLogger, DiscordLogger>();
        return services;
    }
}
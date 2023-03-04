using Microsoft.Extensions.DependencyInjection;

namespace Discord.Logger;

public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddDiscordLogger(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDiscordLogger, DiscordLogger>();
        return serviceCollection;
    }
}
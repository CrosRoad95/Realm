using Microsoft.Extensions.DependencyInjection;

namespace Realm.Module.WebApp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebAppModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<WebAppIntegration>();
        return serviceCollection;
    }
}

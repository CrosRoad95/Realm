using Microsoft.Extensions.DependencyInjection;
using RealmCore.Interfaces.Extend;

namespace RealmCore.Module.WebApp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebAppModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IModule, WebAppIntegration>();
        return serviceCollection;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Realm.Interfaces.Extend;

namespace Realm.Module.WebApp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebAppModule(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IModule, WebAppIntegration>();
        return serviceCollection;
    }
}

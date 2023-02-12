using Microsoft.Extensions.DependencyInjection;

namespace Realm.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmConfiguration(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IRealmConfigurationProvider, RealmConfigurationProvider>();
        return serviceCollection;
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Realm.Identity;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmIdentity<T>(this IServiceCollection services) where T: Db<T>
    {
        services.AddIdentity<User, Role>()
           .AddEntityFrameworkStores<T>()
           .AddDefaultTokenProviders();
        return services;
    }
}
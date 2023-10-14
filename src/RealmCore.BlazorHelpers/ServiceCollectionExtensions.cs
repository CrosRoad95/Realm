using RealmCore.Server.Interfaces;

namespace RealmCore.BlazorHelpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmServer<T>(this IServiceCollection services, T realmServer) where T : class, IRealmServer
    {
        services.AddSingleton<IRealmServer>(realmServer);
        services.AddSingleton(realmServer);
        services.AddScoped(typeof(IRealmService<>), typeof(RealmService<>));
        services.AddHostedService<RealmServerHostedService>();
        return services;
    }
}

using RealmCore.Server;

namespace RealmCore.BlazorHelpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmServer<T>(this IServiceCollection services, T realmServer) where T : RealmServer
    {
        services.AddSingleton<RealmServer>(realmServer);
        services.AddSingleton(realmServer);
        services.AddScoped(typeof(IRealmService<>), typeof(RealmService<>));
        services.AddHostedService<RealmServerHostedService>();
        return services;
    }
}

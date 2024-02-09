using RealmCore.Server.Modules.Server;
using SlipeServer.Server;

namespace RealmCore.BlazorHelpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmServer<T>(this IServiceCollection services, T realmServer) where T : RealmServer
    {
        services.AddSingleton<MtaServer>(x => x.GetRequiredService<T>());
        services.AddSingleton<RealmServer>(x => x.GetRequiredService<T>());
        services.AddSingleton(realmServer);
        services.AddScoped(typeof(IRealmService<>), typeof(RealmService<>));
        services.AddScoped(typeof(IRealmPlayerService<>), typeof(RealmPlayerService<>));
        services.AddHostedService<RealmServerHostedService>();
        return services;
    }
}

namespace RealmCore.BlazorHelpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmServer<T>(this IServiceCollection services, T realmServer) where T : RealmServer
    {
        services.AddSingleton<MtaServer>(x => x.GetRequiredService<T>());
        services.AddSingleton(realmServer);
        services.AddScoped(typeof(IRealmService<>), typeof(RealmService<>));
        services.AddHostedService<RealmServerHostedService>();
        services.AddHostedService<ExternalModulesHostedService>();
        return services;
    }

    public static IServiceCollection AddRealmServer<T>(this IServiceCollection services, RealmServer<T> realmServer) where T : RealmPlayer
    {
        services.AddSingleton<MtaServer>(x => x.GetRequiredService<RealmServer<T>>());
        services.AddSingleton(realmServer);
        services.AddScoped(typeof(IRealmService<>), typeof(RealmService<>));
        services.AddHostedService<RealmServerHostedService>();
        services.AddHostedService<ExternalModulesHostedService>();
        return services;
    }
}

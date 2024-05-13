namespace RealmCore.BlazorHelpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmServer<T>(this IServiceCollection services, Func<IServiceProvider, T> factory) where T : RealmServer
    {
        services.AddSingleton<MtaServer>(x => x.GetRequiredService<T>());
        services.AddSingleton(factory);
        services.AddScoped(typeof(IRealmService<>), typeof(RealmService<>));
        services.AddHostedService<RealmServerHostedService>();
        services.AddHostedService<ExternalModulesHostedService>();
        return services;
    }

    public static IServiceCollection AddRealmServer<T>(this IServiceCollection services, Func<IServiceProvider, RealmServer<T>> factory) where T : RealmPlayer
    {
        services.AddSingleton<MtaServer>(x => x.GetRequiredService<RealmServer<T>>());
        services.AddSingleton(factory);
        services.AddScoped(typeof(IRealmService<>), typeof(RealmService<>));
        services.AddHostedService<RealmServerHostedService>();
        services.AddHostedService<ExternalModulesHostedService>();
        return services;
    }
}

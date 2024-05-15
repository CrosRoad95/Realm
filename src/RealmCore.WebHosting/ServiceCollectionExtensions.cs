using Microsoft.Extensions.DependencyInjection.Extensions;
using SlipeServer.Hosting;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.ServerBuilders;
using RealmCore.Server.Extensions;

namespace RealmCore.WebHosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmServer<TPlayer>(this IServiceCollection services, IConfiguration configuration, Action<ServerBuilder>? builder = null) where TPlayer : RealmPlayer
    {
        var realmConfiguration = configuration.GetRequiredSection("Server").Get<Configuration>();
        services.AddDefaultMtaServerServices();
        services.AddSingleton<IResourceServer, BasicHttpServer>();
        services.AddHostedService<ResourcesServerHostedService>();
        services.TryAddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
        services.ConfigureRealmServices(configuration);

        services.AddMtaServer<TPlayer>(realmConfiguration!, serverBuilder =>
        {
            serverBuilder.AddDefaultServices();
            serverBuilder.AddDefaultLuaMappings();
            serverBuilder.AddDefaultNetWrapper();
            serverBuilder.AddResources();
            builder?.Invoke(serverBuilder);
        });
        //services.AddSingleton<MtaServer>(x => x.GetRequiredService<T>());
        //services.AddSingleton(factory);
        //services.AddScoped(typeof(IRealmService<>), typeof(RealmService<>));
        //services.AddHostedService<RealmServerHostedService>();
        //services.AddHostedService<ExternalModulesHostedService>();
        return services;
    }
}

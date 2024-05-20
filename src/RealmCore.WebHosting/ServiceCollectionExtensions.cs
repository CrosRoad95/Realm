using Microsoft.Extensions.DependencyInjection.Extensions;
using SlipeServer.Hosting;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.ServerBuilders;
using RealmCore.Server.Extensions;

namespace RealmCore.WebHosting;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddRealmServer<TPlayer>(this IHostApplicationBuilder builder, IConfiguration configuration, Action<ServerBuilder>? serverBuilder = null) where TPlayer : RealmPlayer
    {
        builder.ConfigureMtaServers(configure =>
        {
            var isDevelopment = builder.Environment.IsDevelopment();
            var exceptBehaviours = isDevelopment ? ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour : ServerBuilderDefaultBehaviours.None;

            var except = ServerBuilderDefaultPacketHandlers.ExplosionPacketHandler;
            configure.AddDefaultPacketHandlers(except);
            configure.AddDefaultBehaviours(exceptBehaviours);
        });

        builder.Services.AddRealmServer<TPlayer>(configuration, serverBuilder);
        return builder;
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmServer<TPlayer>(this IServiceCollection services, IConfiguration configuration, Action<ServerBuilder>? builder = null) where TPlayer : RealmPlayer
    {
        var realmConfiguration = configuration.GetRequiredSection("Server").Get<Configuration>()!;
        services.AddDefaultMtaServerServices();
        services.AddSingleton<BasicHttpServer>();
        services.TryAddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());

        services.AddMtaServer<TPlayer>(realmConfiguration, serverBuilder =>
        {
            serverBuilder.UseConfiguration(realmConfiguration);
            serverBuilder.AddDefaultServices();
            serverBuilder.AddDefaultLuaMappings();
            serverBuilder.AddDefaultNetWrapper();
            serverBuilder.AddResources();
            builder?.Invoke(serverBuilder);
        });

        services.ConfigureRealmServices(configuration);
        //services.AddSingleton<MtaServer>(x => x.GetRequiredService<T>());
        //services.AddSingleton(factory);
        //services.AddScoped(typeof(IRealmService<>), typeof(RealmService<>));
        //services.AddHostedService<RealmServerHostedService>();
        //services.AddHostedService<ExternalModulesHostedService>();
        return services;
    }
}

using Microsoft.Extensions.DependencyInjection.Extensions;
using SlipeServer.Hosting;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.ServerBuilders;
using RealmCore.Server.Extensions;

namespace RealmCore.WebHosting;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddRealmServer<TPlayer>(this IHostApplicationBuilder builder, Action<ServerBuilder>? serverBuilder = null) where TPlayer : RealmPlayer
    {
        builder.ConfigureMtaServers(configure =>
        {
            var isDevelopment = builder.Environment.IsDevelopment();
            var exceptBehaviours = isDevelopment ? ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour : ServerBuilderDefaultBehaviours.None;

            var except = ServerBuilderDefaultPacketHandlers.ExplosionPacketHandler;
            configure.AddDefaultPacketHandlers(except);
            configure.AddDefaultBehaviours(exceptBehaviours | ServerBuilderDefaultBehaviours.DefaultChatBehaviour);
        });

        builder.Services.AddRealmServer<TPlayer>(builder.Configuration, serverBuilder);
        return builder;
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmServerCore(this IServiceCollection services, IConfiguration configuration)
    {
        var realmConfiguration = configuration.GetRequiredSection("Server").Get<Configuration>()!;
        services.AddDefaultMtaServerServices();
        services.AddSingleton<BasicHttpServer>();
        services.TryAddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());

        services.ConfigureRealmServicesCore(configuration);
        //services.AddSingleton<MtaServer>(x => x.GetRequiredService<T>());
        //services.AddSingleton(factory);
        //services.AddScoped(typeof(IRealmService<>), typeof(RealmService<>));
        //services.AddHostedService<RealmServerHostedService>();
        //services.AddHostedService<ExternalModulesHostedService>();
        return services;
    }
    
    public static IServiceCollection AddRealmServer<TPlayer>(this IServiceCollection services, IConfiguration configuration,  Action<ServerBuilder>? builder = null) where TPlayer : RealmPlayer
    {
        var realmConfiguration = configuration.GetRequiredSection("Server").Get<Configuration>()!;

        services.AddRealmServerCore(configuration);
        services.ConfigureRealmServices();
        services.AddMtaServer(realmConfiguration, services => new MtaDiPlayerServer<TPlayer>(services, realmConfiguration), serverBuilder =>
        {
            serverBuilder.UseConfiguration(realmConfiguration);
            serverBuilder.AddDefaultServices();
            serverBuilder.AddDefaultLuaMappings();
            serverBuilder.AddDefaultNetWrapper();
            serverBuilder.AddResources();
            builder?.Invoke(serverBuilder);
        });

        return services;
    }
}

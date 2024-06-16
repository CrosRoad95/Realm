using Microsoft.Extensions.DependencyInjection.Extensions;
using SlipeServer.Hosting;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.ServerBuilders;
using RealmCore.Server.Extensions;

namespace RealmCore.WebHosting;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddRealmServer<TPlayer>(this IHostApplicationBuilder builder, Action<ServerBuilder>? outerServerBuilder = null) where TPlayer : RealmPlayer
    {
        builder.Services.AddRealmServerCore(builder.Configuration);

        builder.AddMtaServerWithDiSupport<TPlayer>(serverBuilder =>
        {
            var configuration = builder.Configuration.GetRequiredSection("Server").Get<Configuration>()!;
            var isDevelopment = builder.Environment.IsDevelopment();
            var exceptBehaviours = isDevelopment ? ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour : ServerBuilderDefaultBehaviours.None;

            serverBuilder.UseConfiguration(configuration!);
            serverBuilder.AddHostedDefaults(exceptBehaviours: exceptBehaviours | ServerBuilderDefaultBehaviours.DefaultChatBehaviour);

            serverBuilder.AddDefaultServices();
            serverBuilder.AddDefaultLuaMappings();
            serverBuilder.AddResources();
            outerServerBuilder?.Invoke(serverBuilder);
        });

        return builder;
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmServerCore(this IServiceCollection services, IConfiguration configuration)
    {
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
}

using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.ServerBuilders;

namespace RealmCore.Resources.AFK;

public static class ServerBuilderExtensions
{
    public static void AddAFKResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new AFKResource(server);

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddAFKServices();
        });

        builder.AddLogic<AFKLogic>();
    }

    public static IServiceCollection AddAFKServices(this IServiceCollection services)
    {
        services.AddSingleton<IAFKService, AFKService>();
        return services;
    }
}

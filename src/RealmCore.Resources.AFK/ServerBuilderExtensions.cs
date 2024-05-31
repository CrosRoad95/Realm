using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Resources.Base;
using SlipeServer.Server.ServerBuilders;

namespace RealmCore.Resources.AFK;

public static class ServerBuilderExtensions
{
    public static void AddAFKResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new AFKResource(server);
            var additionalFiles = resource.GetAndAddLuaFiles();
            server.AddAdditionalResource(resource, additionalFiles);
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

using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Resources.Base;
using SlipeServer.Server.ServerBuilders;

namespace RealmCore.Resources.Nametags;

public static class ServerBuilderExtensions
{
    public static void AddNametagsResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new NametagsResource(server);
            var additionalFiles = resource.GetAndAddLuaFiles();
            server.AddAdditionalResource(resource, additionalFiles);
            resource.AddLuaEventHub<INametagsEventHub>();
        });

        builder.ConfigureServices(services =>
        {
            services.AddNametagsServices();
        });

        builder.AddLogic<NametagsLogic>();
    }

    public static IServiceCollection AddNametagsServices(this IServiceCollection services)
    {
        services.AddLuaEventHub<INametagsEventHub, NametagsResource>();
        services.AddSingleton<INametagsService, NametagsService>();
        return services;
    }
}

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

            resource.AddLuaEventHub<INametagsEventHub>();
            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddNametagsServices();
        });

        builder.AddLuaEventHub<INametagsEventHub, NametagsResource>();

        builder.AddLogic<NametagsLogic>();
    }

    public static IServiceCollection AddNametagsServices(this IServiceCollection services)
    {
        services.AddSingleton<INametagsService, NametagsService>();
        return services;
    }
}

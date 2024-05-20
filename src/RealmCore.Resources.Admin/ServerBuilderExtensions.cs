using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Resources.Base;
using SlipeServer.Server.ServerBuilders;

namespace RealmCore.Resources.Admin;

public static class ServerBuilderExtensions
{
    public static void AddAdminResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new AdminResource(server);

            resource.AddLuaEventHub<IAdminEventHub>();
            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddAdminServices();
        });

        builder.AddLogic<AdminLogic>();
    }

    public static IServiceCollection AddAdminServices(this IServiceCollection services)
    {
        services.AddLuaEventHub<IAdminEventHub, AdminResource>();
        services.AddSingleton<IAdminService, AdminService>();
        return services;
    }
}

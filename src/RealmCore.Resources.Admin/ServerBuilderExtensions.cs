using Microsoft.Extensions.DependencyInjection;
using RealmCore.Resources.Base;
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
            services.AddSingleton<IAdminService, AdminService>();
        });

        builder.AddLuaEventHub<IAdminEventHub, AdminResource>();
        builder.AddLogic<AdminLogic>();
    }
}

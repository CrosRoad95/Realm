using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.ServerBuilders;

namespace Realm.Resources.AdminTools;

public static class ServerBuilderExtensions
{
    public static void AddAdminToolsResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new AdminToolsResource(server);

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IAdminToolsService, AdminToolsService>();
        });

        builder.AddLogic<AdminToolsLogic>();
    }
}

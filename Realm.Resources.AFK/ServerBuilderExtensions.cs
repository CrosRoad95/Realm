using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.ServerBuilders;

namespace Realm.Resources.AFK;

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
            services.AddSingleton<AFKService>();
        });

        builder.AddLogic<AFKLogic>();
    }
}

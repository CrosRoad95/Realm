using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.ServerBuilders;

namespace Realm.Resources.Nametags;

public static class ServerBuilderExtensions
{
    public static void AddNametagsResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new NametagsResource(server);

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<NametagsService>();
        });

        builder.AddLogic<NametagsLogic>();
    }
}

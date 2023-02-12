using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.ServerBuilders;

namespace Realm.Resources.Assets;

public static class ServerBuilderExtensions
{
    public static void AddAssetsResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new AssetsResource(server);

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<AssetsService>();
            services.AddSingleton<AssetsRegistry>();
        });

        builder.AddLogic<AssetsLogic>();
    }
}

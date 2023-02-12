using Microsoft.Extensions.DependencyInjection;
using Realm.Resources.Assets;
using SlipeServer.Server.ServerBuilders;

namespace Realm.Resources.Overlay;

public static class ServerBuilderExtensions
{
    public static void AddOverlayResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new OverlayResource(server);
            resource.InjectAssetsExportedFunctions();

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<OverlayService>();
        });

        builder.AddLogic<OverlayLogic>();
    }
}

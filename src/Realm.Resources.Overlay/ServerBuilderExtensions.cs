using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.ServerBuilders;

namespace Realm.Resources.Overlay;

public static class ServerBuilderExtensions
{
    public static void AddOverlayResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new OverlayResource(server);

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<OverlayService>();
        });

        builder.AddLogic<OverlayLogic>();
    }
}

[assembly: InternalsVisibleTo("RealmCore.Tests")]

namespace RealmCore.Resources.Overlay;

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
            services.AddSingleton<IOverlayService, OverlayService>();
        });

        builder.AddLogic<OverlayLogic>();
    }
}

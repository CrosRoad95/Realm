using SlipeServer.Resources.Base;

[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]


namespace RealmCore.Resources.Overlay;

public static class ServerBuilderExtensions
{
    public static void AddOverlayResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new OverlayResource(server);
            resource.AddLuaEventHub<IHudEventHub>();

            resource.InjectAssetsExportedFunctions();

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddOverlayServices();
        });

        builder.AddLogic<OverlayLogic>();
    }

    public static IServiceCollection AddOverlayServices(this IServiceCollection services)
    {
        services.AddLuaEventHub<IHudEventHub, OverlayResource>();
        services.AddSingleton<IOverlayService, OverlayService>();
        return services;
    }
}

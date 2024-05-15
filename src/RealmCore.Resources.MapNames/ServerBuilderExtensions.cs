[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]

namespace RealmCore.Resources.MapNames;

public static class ServerBuilderExtensions
{
    public static void AddMapNamesResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new MapNamesResource(server);
            resource.AddLuaEventHub<IMapNamesEventHub>();

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddMapNamesServices();
        });

        builder.AddLuaEventHub<IMapNamesEventHub, MapNamesResource>();
        builder.AddLogic<MapNamesLogic>();
    }

    public static IServiceCollection AddMapNamesServices(this IServiceCollection services)
    {
        services.AddSingleton<IMapNamesService, MapNamesService>();
        return services;
    }
}

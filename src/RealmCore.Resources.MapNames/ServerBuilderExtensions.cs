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
            var additionalFiles = resource.GetAndAddLuaFiles();
            server.AddAdditionalResource(resource, additionalFiles);
            resource.AddLuaEventHub<IMapNamesEventHub>();
        });

        builder.ConfigureServices(services =>
        {
            services.AddMapNamesServices();
        });

        builder.AddLogic<MapNamesLogic>();
    }

    public static IServiceCollection AddMapNamesServices(this IServiceCollection services)
    {
        services.AddLuaEventHub<IMapNamesEventHub, MapNamesResource>();
        services.AddSingleton<IMapNamesService, MapNamesService>();
        return services;
    }
}

using SlipeServer.Resources.Base;

[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]

namespace RealmCore.Resources.ElementOutline;

public static class ServerBuilderExtensions
{
    public static void AddElementOutlineResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new ElementOutlineResource(server);
            var additionalFiles = resource.GetAndAddLuaFiles();
            server.AddAdditionalResource(resource, additionalFiles);

            resource.AddLuaEventHub<IElementOutlineEventHub>();
        });

        builder.ConfigureServices(services =>
        {
            services.AddElementOutlineServices();
        });

        builder.AddLogic<ElementOutlineLogic>();
    }

    public static IServiceCollection AddElementOutlineServices(this IServiceCollection services)
    {
        services.AddLuaEventHub<IElementOutlineEventHub, ElementOutlineResource>();
        services.AddSingleton<IElementOutlineService, ElementOutlineService>();
        return services;
    }
}


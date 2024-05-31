[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]

namespace RealmCore.Resources.ClientInterface;

public static class ServerBuilderExtensions
{
    public static void AddClientInterfaceResource(this ServerBuilder builder, CommonResourceOptions? commonResourceOptions = null)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new ClientInterfaceResource(server);
            if (commonResourceOptions != null)
                commonResourceOptions.Configure(resource);

            var additionalFiles = resource.GetAndAddLuaFiles();
            server.AddAdditionalResource(resource, additionalFiles);
            resource.AddLuaEventHub<IClientInterfaceEventHub>();
        });

        builder.ConfigureServices(services =>
        {
            services.AddClientInterfaceServices();
        });

        builder.AddLogic<ClientInterfaceLogic>();
    }

    public static IServiceCollection AddClientInterfaceServices(this IServiceCollection services)
    {
        services.AddLuaEventHub<IClientInterfaceEventHub, ClientInterfaceResource>();
        services.AddSingleton<IClientInterfaceService, ClientInterfaceService>();
        return services;
    }
}

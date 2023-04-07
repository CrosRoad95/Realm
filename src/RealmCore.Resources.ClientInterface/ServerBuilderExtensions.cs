using Microsoft.Extensions.DependencyInjection;
using RealmCore.Resources.Base;
using SlipeServer.Server.ServerBuilders;

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

            resource.AddLuaEventHub<IClientInterfaceEventHub>();
            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IClientInterfaceService, ClientInterfaceService>();
        });

        builder.AddLuaEventHub<IClientInterfaceEventHub, ClientInterfaceResource>();
        builder.AddLogic<ClientInterfaceLogic>();
    }
}

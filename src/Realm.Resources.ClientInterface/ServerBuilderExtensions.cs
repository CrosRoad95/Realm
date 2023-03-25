using Microsoft.Extensions.DependencyInjection;
using Realm.Resources.Addons.Common;
using SlipeServer.Server.ServerBuilders;

namespace Realm.Resources.ClientInterface;

public static class ServerBuilderExtensions
{
    public static void AddClientInterfaceResource(this ServerBuilder builder, CommonResourceOptions? commonResourceOptions = null)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new ClientInterfaceResource(server);
            if (commonResourceOptions != null)
                commonResourceOptions.Configure(resource);

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IClientInterfaceService, ClientInterfaceService>();
        });

        builder.AddLogic<ClientInterfaceLogic>();
    }
}

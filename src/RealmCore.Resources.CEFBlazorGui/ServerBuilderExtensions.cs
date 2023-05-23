using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.ServerBuilders;

namespace RealmCore.Resources.CEFBlazorGui;

public static class ServerBuilderExtensions
{
    public static void AddCEFBlazorGuiResource(this ServerBuilder builder, string filesLocation)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new CEFBlazorGuiResource(server, filesLocation);

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<ICEFBlazorGuiService, CEFBlazorGuiService>();
        });

        builder.AddLogic<CEFBlazorGuiLogic>();
    }
}

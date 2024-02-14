using Microsoft.Extensions.DependencyInjection;
using RealmCore.Resources.Base;
using SlipeServer.Server.ServerBuilders;

[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]

namespace RealmCore.Resources.Browser;

public static class ServerBuilderExtensions
{
    public static void AddBrowserResource(this ServerBuilder builder, string? filesLocation = null)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new BrowserResource(server, filesLocation);
            resource.AddLuaEventHub<IBrowserEventHub>();

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IBrowserService, BrowserService>();
        });

        builder.AddLuaEventHub<IBrowserEventHub, BrowserResource>();
        builder.AddLogic<BrowserLogic>();
    }
}

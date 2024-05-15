using Microsoft.Extensions.DependencyInjection;
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
            server.AddAdditionalResource(resource, resource.AdditionalFiles);
            resource.AddLuaEventHub<IBrowserEventHub>();
        });

        builder.ConfigureServices(services =>
        {
            services.AddBrowserServices();
        });

        builder.AddLuaEventHub<IBrowserEventHub, BrowserResource>();
        builder.AddLogic<BrowserLogic>();
    }

    public static IServiceCollection AddBrowserServices(this IServiceCollection services)
    {
        services.AddSingleton<IBrowserService, BrowserService>();
        return services;
    }
}

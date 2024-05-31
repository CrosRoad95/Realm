using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.ServerBuilders;

[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]

namespace RealmCore.Resources.Browser;

public static class ServerBuilderExtensions
{
    public static void AddBrowserResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new BrowserResource(server);
            var additionalFiles = resource.GetAndAddLuaFiles();
            server.AddAdditionalResource(resource, additionalFiles);
            resource.AddLuaEventHub<IBrowserEventHub>();
        });

        builder.ConfigureServices(services =>
        {
            services.AddBrowserServices();
        });

        builder.AddLogic<BrowserLogic>();
    }

    public static IServiceCollection AddBrowserServices(this IServiceCollection services)
    {
        services.AddSingleton<IBrowserService, BrowserService>();
        services.AddLuaEventHub<IBrowserEventHub, BrowserResource>();
        return services;
    }
}

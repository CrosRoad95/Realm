using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Resources.Base;
using SlipeServer.Server.ServerBuilders;

[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace RealmCore.Resources.Assets;

public static class ServerBuilderExtensions
{
    public static void AddAssetsResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new AssetsResource(server);

            var additionalFiles = resource.GetAndAddLuaFiles();
            server.AddAdditionalResource(resource, additionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddAssetsServices();
        });

        builder.AddLogic<AssetsLogic>();
    }

    public static IServiceCollection AddAssetsServices(this IServiceCollection services)
    {
        services.AddSingleton<IAssetsService, AssetsService>();
        services.AddSingleton<AssetsCollection>();
        services.AddSingleton<IServerAssetsProvider>(x => x.GetRequiredService<AssetsCollection>());
        return services;
    }
}

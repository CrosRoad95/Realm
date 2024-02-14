using Microsoft.Extensions.DependencyInjection;
using RealmCore.Resources.Assets.Interfaces;
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

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IAssetsService, AssetsService>();
            services.AddSingleton<AssetsCollection>();
            services.AddSingleton<IServerAssetsProvider>(x => x.GetRequiredService<AssetsCollection>());
        });

        builder.AddLogic<AssetsLogic>();
    }
}

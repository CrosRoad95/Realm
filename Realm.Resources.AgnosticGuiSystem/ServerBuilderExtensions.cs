using Microsoft.Extensions.DependencyInjection;
using Realm.Resources.Addons.Common;
using SlipeServer.Server.ServerBuilders;

namespace Realm.Resources.AgnosticGuiSystem;

public static class ServerBuilderExtensions
{
    public static void AddAgnosticGuiSystemResource(this ServerBuilder builder, Action<AgnosticGuiSystemOptions> optionsBuilder, CommonResourceOptions? commonResourceOptions = null)
    {
        var options = new AgnosticGuiSystemOptions();
        optionsBuilder(options);

        builder.AddBuildStep(server =>
        {
            var resource = new AgnosticGuiSystemResource(server);
            if(commonResourceOptions != null)
                commonResourceOptions.Configure(resource);

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<AgnosticGuiSystemService>();
        });
    }
}

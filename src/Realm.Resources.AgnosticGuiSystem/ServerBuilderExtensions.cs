using Microsoft.Extensions.DependencyInjection;
using Realm.Resources.Addons.Common;
using SlipeServer.Server.ServerBuilders;
using SlipeServer.Resources.DGS;

namespace Realm.Resources.AgnosticGuiSystem;

public static class ServerBuilderExtensions
{
    public static void AddAgnosticGuiSystemResource(this ServerBuilder builder, Action<AgnosticGuiSystemOptions> optionsBuilder, CommonResourceOptions? commonResourceOptions = null)
    {
        var options = new AgnosticGuiSystemOptions();
        optionsBuilder(options);

        if (!options._providers.Any())
            throw new Exception("No gui provider found");

        builder.AddBuildStep(server =>
        {
            var resource = new AgnosticGuiSystemResource(server, options);
            resource.InjectDGSExportedFunctions();
            if (commonResourceOptions != null)
                commonResourceOptions.Configure(resource);

            var additionalFiles = resource.AdditionalFiles
                .Concat(options._providers)
                .ToDictionary(x => x.Key, x => x.Value);
            server.AddAdditionalResource(resource, additionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IAgnosticGuiSystemService, AgnosticGuiSystemService>();
        });

        builder.AddLogic<AgnosticGuiSystemLogic>();
    }
}

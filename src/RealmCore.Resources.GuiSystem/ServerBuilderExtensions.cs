using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.ServerBuilders;
using SlipeServer.Resources.DGS;
using RealmCore.Resources.Base;

namespace RealmCore.Resources.GuiSystem;

public static class ServerBuilderExtensions
{
    public static void AddGuiSystemResource(this ServerBuilder builder, Action<GuiSystemOptions> optionsBuilder, CommonResourceOptions? commonResourceOptions = null)
    {
        var options = new GuiSystemOptions();
        optionsBuilder(options);

        if (options._providers.Count == 0)
            throw new Exception("No gui provider found");

        builder.AddBuildStep(server =>
        {
            var resource = new GuiSystemResource(server, options);
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
            services.AddGuiSystemServices();
        });

        builder.AddLogic<GuiSystemLogic>();
    }

    public static IServiceCollection AddGuiSystemServices(this IServiceCollection services)
    {
        services.AddSingleton<IGuiSystemService, GuiSystemService>();
        return services;
    }
}

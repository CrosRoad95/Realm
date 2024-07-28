using SlipeServer.Resources.DiscordRichPresence;

[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace RealmCore.Server.Extensions;

public static class ServerBuilderExtensions
{
    public static ServerBuilder AddResources(this ServerBuilder serverBuilder)
    {
        var commonOptions = new CommonResourceOptions();

        serverBuilder.AddBrowserResource();
        serverBuilder.AddNoClipResource(new NoClipOptions
        {
            Bind = null
        });
        serverBuilder.AddClientInterfaceResource(commonOptions);
        serverBuilder.AddElementOutlineResource();
        serverBuilder.AddAdminResource();
        serverBuilder.AddAFKResource();
        serverBuilder.AddMapNamesResource();
        serverBuilder.AddStatisticsCounterResource();
        serverBuilder.AddOverlayResource();
        serverBuilder.AddAssetsResource();
        serverBuilder.AddText3dResource();
        serverBuilder.AddNametagsResource();
        serverBuilder.AddWatermarkResource();
        serverBuilder.AddBoneAttachResource(BoneAttachVersion.Release_1_2_0);
        serverBuilder.AddDiscordRichPresenceResource(new DiscordRichPresenceOptions
        {
            ApplicationId = 997855189132398613
        });
        return serverBuilder;
    }
    
    public static IServiceCollection AddResources(this IServiceCollection services)
    {
        services.AddBrowserServices();
        services.AddNoClipServices();
        services.AddClientInterfaceServices();
        services.AddElementOutlineServices();
        services.AddAdminServices();
        services.AddAFKServices();
        services.AddMapNamesServices();
        services.AddStatisticsCounterServices();
        services.AddOverlayServices();
        services.AddAssetsServices();
        services.AddText3dServices();
        services.AddNametagsServices();
        services.AddWatermarkServices();
        services.AddBoneAttachServices();
        services.AddDiscordRichPresenceServices();
        return services;
    }
}

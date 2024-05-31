[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace RealmCore.Server.Extensions;

[Flags]
public enum ExcludeResources
{
    BoneAttach = 0x1,
    DGS = 0x2,
}

public static class ServerBuilderExtensions
{
    public static ServerBuilder AddResources(this ServerBuilder serverBuilder, ExcludeResources? excludeResources = null)
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
        serverBuilder.AddScoreboard();
        if (excludeResources == null || !excludeResources.Value.HasFlag(ExcludeResources.BoneAttach))
            serverBuilder.AddBoneAttachResource(BoneAttachVersion.Release_1_2_0);
        //if (excludeResources == null || !excludeResources.Value.HasFlag(ExcludeResources.DGS))
        //{
        //    serverBuilder.AddDGSResource(DGSVersion.Release_3_520);
        //    serverBuilder.AddGuiSystemResource(builder =>
        //    {
        //        builder.AddGuiProvider(DGSGuiProvider.Name, DGSGuiProvider.LuaCode);
        //        builder.SetGuiProvider(DGSGuiProvider.Name);
        //    }, new());
        //}
        return serverBuilder;
    }
    
    public static IServiceCollection AddResources(this IServiceCollection services, ExcludeResources? excludeResources = null)
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
        services.AddScoreboardServices();
        if (excludeResources == null || !excludeResources.Value.HasFlag(ExcludeResources.BoneAttach))
            services.AddBoneAttachServices();
        return services;
    }

    public static IServiceCollection WithGuiSystem(this IServiceCollection services)
    {
        services.AddHostedService<DxGuiSystemHostedService>();
        services.AddHostedService<ReactiveDxGuiHostedService>();
        return services;
    }

}

[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace RealmCore.Server.Extensions;

[Flags]
public enum ExcludeResources
{
    BoneAttach = 0x01
}

public static class ServerBuilderExtensions
{
    public static ServerBuilder AddResources(this ServerBuilder serverBuilder, HttpClient? httpClient = null, ExcludeResources? excludeResources = null)
    {
        var commonOptions = new CommonResourceOptions();

        serverBuilder.AddBrowserResource();
        //serverBuilder.AddNoClipResource(new NoClipOptions
        //{
        //    Bind = null
        //});
        //serverBuilder.AddClientInterfaceResource(commonOptions);
        //serverBuilder.AddElementOutlineResource();
        //serverBuilder.AddAdminResource();
        //serverBuilder.AddAFKResource();
        //serverBuilder.AddMapNamesResource();
        //serverBuilder.AddStatisticsCounterResource();
        //serverBuilder.AddOverlayResource();
        //serverBuilder.AddAssetsResource();
        //serverBuilder.AddText3dResource();
        //serverBuilder.AddNametagsResource();
        //if (excludeResources == null || (excludeResources != null && !excludeResources.Value.HasFlag(ExcludeResources.BoneAttach)))
        //    serverBuilder.AddBoneAttachResource(BoneAttachVersion.Release_1_2_0, httpClient);
        //serverBuilder.AddWatermarkResource();
        //serverBuilder.AddScoreboard();
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
        if (excludeResources == null || (excludeResources != null && !excludeResources.Value.HasFlag(ExcludeResources.BoneAttach)))
            services.AddBoneAttachServices();
        services.AddWatermarkServices();
        services.AddScoreboardServices();
        return services;
    }

    public static ServerBuilder ConfigureServer(this ServerBuilder serverBuilder, IConfiguration configuration, ServerBuilderDefaultBehaviours? serverBuilderDefaultBehaviours = null, HttpClient? httpClient = null, ExcludeResources? excludeResources = null)
    {
        var _serverConfiguration = configuration.GetSection("Server").Get<Configuration>();
        if (_serverConfiguration == null)
            throw new InvalidOperationException("Server configuration is null");

        serverBuilder.UseConfiguration(_serverConfiguration);
        if (serverBuilderDefaultBehaviours != null)
        {
            if (serverBuilderDefaultBehaviours != ServerBuilderDefaultBehaviours.None)
            {
                serverBuilder.AddDefaults(exceptBehaviours: serverBuilderDefaultBehaviours.Value);
            }
        }
        else
        {
            serverBuilder.AddResourceServer<RealmResourceServer>();
            var exceptBehaviours = ServerBuilderDefaultBehaviours.DefaultChatBehaviour | ServerBuilderDefaultBehaviours.NicknameChangeBehaviour;
#if DEBUG
            serverBuilder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour | exceptBehaviours);
#else
            serverBuilder.AddDefaults(exceptBehaviours: exceptBehaviours);
#endif
            serverBuilder.AddBehaviour<CollisionShapeBehaviour>();
        }

        #region Behaviours
        serverBuilder.AddBehaviour<ScopedCollisionShapeBehaviour>();
        #endregion

        return serverBuilder;
    }

    public static IServiceCollection WithGuiSystem(this IServiceCollection services)
    {
        services.AddHostedService<DxGuiSystemHostedService>();
        services.AddHostedService<ReactiveDxGuiHostedService>();
        return services;
    }

}

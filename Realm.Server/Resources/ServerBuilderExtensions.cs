namespace Realm.Server.Resources;

internal static class ServerBuilderExtensions
{
    public static void AddResource<TResource, TResourceLogicInterface, TResourceLogic>(this ServerBuilder builder)
        where TResource: ResourceBase, IResourceConstructor
        where TResourceLogicInterface : class
        where TResourceLogic : class, TResourceLogicInterface
    {
        builder.AddBuildStep(server =>
        {
            Dictionary<string, byte[]> additionalFiles = ResourceBase.GetAdditionalFiles<TResource>();
            var resource = (TResource)TResource.Create<TResource>(server, additionalFiles);
            server.AddAdditionalResource(resource, additionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<TResourceLogicInterface, TResourceLogic>();
        });
        builder.AddLogic<TResourceLogic>();
    }

    public static void AddResourceWithAutostart<TResource, TResourceLogicInterface, TResourceLogic>(this ServerBuilder builder)
        where TResource: ResourceBase, IResourceConstructor
        where TResourceLogicInterface : class
        where TResourceLogic : class, TResourceLogicInterface, IAutoStartResource
    {
        builder.AddBuildStep(server =>
        {
            Dictionary<string, byte[]> additionalFiles = ResourceBase.GetAdditionalFiles<TResource>();
            var resource = (TResource)TResource.Create<TResource>(server, additionalFiles);
            server.AddAdditionalResource(resource, additionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<TResourceLogicInterface, TResourceLogic>();
            services.AddSingleton<IAutoStartResource, TResourceLogic>();
        });
        builder.AddLogic<TResourceLogic>();
    }
}

using Microsoft.Extensions.DependencyInjection;
using Realm.Resources.Addons.Common;
using SlipeServer.Server.ServerBuilders;

namespace Realm.Resources.LuaInterop;

public static class ServerBuilderExtensions
{
    public static void AddLuaInteropResource(this ServerBuilder builder, CommonResourceOptions? commonResourceOptions = null)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new LuaInteropResource(server);
            if (commonResourceOptions != null)
                commonResourceOptions.Configure(resource);

            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<LuaInteropService>();
        });

        builder.AddLogic<LuaInteropLogic>();
    }
}

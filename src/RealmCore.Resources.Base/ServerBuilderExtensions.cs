using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.Resources;
using SlipeServer.Server.ServerBuilders;

namespace RealmCore.Resources.Base;

public static class ServerBuilderExtensions
{
    public static ServerBuilder AddLuaEventHub<THub, TResource>(this ServerBuilder builder) where TResource : Resource
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<ILuaEventHub<THub>, LuaEventHub<THub, TResource>>();
        });
        return builder;
    }
}
